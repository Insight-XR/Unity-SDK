using System.Diagnostics;
using Mono.Cecil;
using Mono.Cecil.Rocks;

namespace Burst.Compiler.IL.Syntax
{
    /// <summary>
    /// A generic context contains a mapping between GenericParameter ({T}) and resolved TypeReference (int, float, MyStruct&lt;float&gt;)
    /// </summary>
#if BURST_INTERNAL || BURST_COMPILER_SHARED
    public
#else
    internal
#endif
    readonly struct GenericContext
    {
        private readonly GenericInstanceType _typeContext;
        private readonly GenericInstanceMethod _methodContext;

        /// <summary>
        /// An empty <see cref="GenericContext"/>
        /// </summary>
        public static readonly GenericContext None = new GenericContext();

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericContext"/> class.
        /// </summary>
        /// <param name="genericMethod">The generic method instance.</param>
        private GenericContext(GenericInstanceMethod genericMethod, GenericInstanceType genericType)
        {
            _methodContext = genericMethod;
            _typeContext = genericType;
        }

        /// <summary>
        /// Is there no generics in this context?
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return _typeContext == null && _methodContext == null;
        }

        /// <summary>
        /// Resolve the generics of the given <see cref="T:Mono.Cecil.MethodReference"/>
        /// </summary>
        /// <param name="unresolvedMethod"></param>
        /// <returns></returns>
        public MethodReference Resolve(MethodReference unresolvedMethod)
        {
            Debug.Assert(unresolvedMethod != null);

            // The following code was originally derived from IL2CPP.
            var resolvedMethod = unresolvedMethod;

            if (IsEmpty())
            {
                return resolvedMethod;
            }

            var declaringType = Resolve(unresolvedMethod.DeclaringType);

            if (unresolvedMethod is GenericInstanceMethod genericInstanceMethod)
            {
                resolvedMethod = new MethodReference(unresolvedMethod.Name, unresolvedMethod.ReturnType, declaringType);

                foreach (var p in unresolvedMethod.Parameters)
                {
                    resolvedMethod.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, p.ParameterType));
                }

                foreach (var gp in genericInstanceMethod.ElementMethod.GenericParameters)
                {
                    resolvedMethod.GenericParameters.Add(new GenericParameter(gp.Name, resolvedMethod));
                }

                resolvedMethod.HasThis = unresolvedMethod.HasThis;

                var m = new GenericInstanceMethod(resolvedMethod);

                foreach (var ga in genericInstanceMethod.GenericArguments)
                {
                    m.GenericArguments.Add(Resolve(ga));
                }

                resolvedMethod = m;
            }
            else
            {
                if (unresolvedMethod.HasGenericParameters)
                {
                    var newGenericInstanceMethod = new GenericInstanceMethod(unresolvedMethod);

                    foreach (var gp in unresolvedMethod.GenericParameters)
                    {
                        newGenericInstanceMethod.GenericArguments.Add(Resolve(gp));
                    }

                    resolvedMethod = newGenericInstanceMethod;
                }
                else
                {
                    resolvedMethod = new MethodReference(unresolvedMethod.Name, unresolvedMethod.ReturnType, declaringType);

                    foreach (var p in unresolvedMethod.Parameters)
                    {
                        resolvedMethod.Parameters.Add(new ParameterDefinition(p.Name, p.Attributes, p.ParameterType));
                    }

                    resolvedMethod.HasThis = unresolvedMethod.HasThis;
                    resolvedMethod.MetadataToken = unresolvedMethod.MetadataToken;
                }
            }

            return resolvedMethod;
        }

        /// <summary>
        /// Expands the specified <see cref="T:Mono.Cecil.TypeReference"/> if it is either a <see cref="T:Mono.Cecil.GenericParameter"/> or a partially expanded <see cref="T:Mono.Cecil.GenericInstanceType"/>
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns>TypeReference.</returns>
        public TypeReference Resolve(TypeReference typeReference)
        {
            Debug.Assert(typeReference != null);

            if (IsEmpty())
            {
                return typeReference;
            }

            switch (typeReference)
            {
                case GenericParameter genericParam:
                    Debug.Assert(genericParam.Owner != null);

                    if (genericParam.Owner.GenericParameterType == GenericParameterType.Type)
                    {
                        Debug.Assert(_typeContext != null);
                        return _typeContext.GenericArguments[genericParam.Position];
                    }
                    else
                    {
                        Debug.Assert(_methodContext != null);
                        return _methodContext.GenericArguments[genericParam.Position];
                    }
                case ArrayType arrayType:
                    return new ArrayType(Resolve(arrayType.ElementType), arrayType.Rank);
                case PointerType pointerType:
                    return Resolve(pointerType.ElementType).MakePointerType();
                case PinnedType pinnedType:
                    return Resolve(pinnedType.ElementType).MakePointerType();
                case ByReferenceType byRefType:
                    return Resolve(byRefType.ElementType).MakeByReferenceType();
                case RequiredModifierType requiredModType:
                    return new RequiredModifierType(requiredModType.ModifierType, Resolve(requiredModType.ElementType));
                case OptionalModifierType optionalModType:
                    return Resolve(optionalModType.ElementType);
            }

            if (ContainsGenericParameters(typeReference))
            {
                if (typeReference is GenericInstanceType partialGenericInstance)
                {
                    // TODO: Ideally, we should cache this GenericInstanceType once it has been resolved
                    var genericInstance = new GenericInstanceType(partialGenericInstance.ElementType);
                    foreach (var genericArgument in partialGenericInstance.GenericArguments)
                    {
                        genericInstance.GenericArguments.Add(Resolve(genericArgument));
                    }
                    return genericInstance;
                }
                else
                {
                    // Sometimes we can have a TypeDefinition with HasGenericParameters false, but GenericParameters.Count > 0
                    var typeDefinition = typeReference as TypeDefinition;
                    if (typeDefinition?.GenericParameters.Count > 0)
                    {
                        var genericInstance = new GenericInstanceType(typeDefinition);
                        foreach (var genericArgument in typeDefinition.GenericParameters)
                        {
                            genericInstance.GenericArguments.Add(Resolve(genericArgument));
                        }
                        return genericInstance;
                    }
                }
            }

            return typeReference;
        }

        /// <summary>
        /// If the given type is a reference or pointer type, the underlying type is returned
        /// </summary>
        /// <param name="typeReference"></param>
        /// <returns></returns>
        public static TypeReference GetTypeReferenceForPointerOrReference(TypeReference typeReference)
        {
            while (true)
            {
                switch (typeReference)
                {
                    case PointerType pointerType:
                        typeReference = pointerType.ElementType;
                        break;
                    case ByReferenceType byRefType:
                        typeReference = byRefType.ElementType;
                        break;
                    default:
                        return typeReference;
                }
            }
        }

        /// <summary>
        /// Create <see cref="GenericContext"/> from a <see cref="T:Mono.Cecil.TypeReference"/>
        /// </summary>
        /// <param name="typeReference"></param>
        /// <returns></returns>
        public static GenericContext From(TypeReference typeReference)
        {
            Debug.Assert(typeReference != null);

            if (typeReference is PinnedType pinnedType)
            {
                typeReference = pinnedType.ElementType;
            }

            typeReference = GetTypeReferenceForPointerOrReference(typeReference);

            if (typeReference is ArrayType arrayType)
            {
                typeReference = arrayType.ElementType;
            }

            return new GenericContext(null, typeReference as GenericInstanceType);
        }

        /// <summary>
        /// Create <see cref="GenericContext"/> from a <see cref="T:Mono.Cecil.MethodReference"/> and a <see cref="T:Mono.Cecil.TypeReference"/>
        /// </summary>
        /// <param name="methodReference"></param>
        /// <param name="typeReference"></param>
        /// <returns></returns>
        public static GenericContext From(MethodReference methodReference, TypeReference typeReference)
        {
            Debug.Assert(methodReference != null);
            Debug.Assert(typeReference != null);

            typeReference = GetTypeReferenceForPointerOrReference(typeReference);

            return new GenericContext(methodReference as GenericInstanceMethod, typeReference as GenericInstanceType);
        }

        /// <summary>
        /// Checks if the specified TypeReference contains generic parameters that need type expansion
        /// </summary>
        /// <param name="typeReference">The type reference.</param>
        /// <returns><c>true</c> if the specified TypeReference contains generic arguments that need type expansion, <c>false</c> otherwise.</returns>
        public static bool ContainsGenericParameters(TypeReference typeReference)
        {
            switch (typeReference)
            {
                case GenericParameter genericParam:
                    return true;
                case ArrayType arrayType:
                    return ContainsGenericParameters(arrayType.ElementType);
                case PointerType pointerType:
                    return ContainsGenericParameters(pointerType.ElementType);
                case PinnedType pinnedType:
                    return ContainsGenericParameters(pinnedType.ElementType);
                case ByReferenceType byRefType:
                    return ContainsGenericParameters(byRefType.ElementType);
                case RequiredModifierType requiredModType:
                    return ContainsGenericParameters(requiredModType.ModifierType);
                case OptionalModifierType optionalModType:
                    return ContainsGenericParameters(optionalModType.ElementType);
                case GenericInstanceType partialGenericInstance:
                {
                    foreach (var genericArgument in partialGenericInstance.GenericArguments)
                    {
                        if (ContainsGenericParameters(genericArgument))
                        {
                            return true;
                        }
                    }

                    break;
                }

                case TypeDefinition typeDefinition:
                {
                    // Sometimes we can have a TypeDefinition with HasGenericParameters false, but GenericParameters.Count > 0
                    return typeDefinition.GenericParameters.Count > 0;
                }
            }

            return false;
        }
    }
}
