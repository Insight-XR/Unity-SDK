using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace zzzUnity.Burst.CodeGen
{
    /// <summary>
    /// Provides some Cecil Extensions.
    /// </summary>
#if BURST_COMPILER_SHARED
    public
#else
    internal
#endif
    static class CecilExtensions
    {
        public static void BuildAssemblyQualifiedName(this TypeReference type, StringBuilder builder)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            TypeReference elementType;
            type.BuildReflectionFullName(builder, out elementType, assemblyQualified: true);

            if (!(elementType is GenericParameter))
            {
                // Recover assembly reference from scope first (e.g for types imported), otherwise from Module.Assembly
                var assemblyReference = elementType.Scope as AssemblyNameReference ?? elementType.Module?.Assembly?.Name;
                if (assemblyReference != null)
                {
                    builder.Append(", ").Append(assemblyReference);
                }
            }
        }

        public static void BuildReflectionFullName(this TypeReference type, StringBuilder builder, bool assemblyQualified)
        {
            BuildReflectionFullName(type, builder, out _, assemblyQualified);
        }

        public static void BuildReflectionFullName(this TypeReference type, StringBuilder builder, out TypeReference elementType, bool assemblyQualified)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            if (type is PointerType pointerType)
            {
                pointerType.ElementType.BuildReflectionFullName(builder, out elementType, assemblyQualified);
                builder.Append("*");
            }
            else if (type is PinnedType pinnedType)
            {
                pinnedType.ElementType.BuildReflectionFullName(builder, out elementType, assemblyQualified);
                builder.Append(" pinned");
            }
            else if (type is ByReferenceType byReferenceType)
            {
                byReferenceType.ElementType.BuildReflectionFullName(builder, out elementType, assemblyQualified);
                builder.Append("&");
            }
            else if (type is ArrayType arrayType)
            {
                arrayType.ElementType.BuildReflectionFullName(builder, out elementType, assemblyQualified);
                builder.Append("[]");
            }
            else if (type is GenericParameter genericParameter)
            {
                elementType = type;
                builder.Append(genericParameter.Type == GenericParameterType.Method ? "!!" : "!");
                builder.Append(genericParameter.Position);
            }
            else if (type is FunctionPointerType functionPointerType)
            {
                elementType = type;
                builder.Append("delegate* ");
                builder.Append(functionPointerType.CallingConvention switch
                {
                    MethodCallingConvention.Default => "managed",
                    MethodCallingConvention.Unmanaged => "unmanaged",
                    MethodCallingConvention.C => "unmanaged[Cdecl]",
                    MethodCallingConvention.FastCall => "unmanaged[Fastcall]",
                    MethodCallingConvention.ThisCall => "unmanaged[Thiscall]",
                    MethodCallingConvention.StdCall => "unmanaged[Stdcall]",
                    MethodCallingConvention.Generic => "generic",
                    MethodCallingConvention.VarArg => "vararg",
                    var conv => $"<unknown calling conv: {(int)conv}>",
                });
                builder.Append("<");
                for (var i = 0; i < functionPointerType.Parameters.Count; i++)
                {
                    var param = functionPointerType.Parameters[i];
                    param.ParameterType.BuildAssemblyQualifiedName(builder);
                    builder.Append(", ");
                }

                functionPointerType.MethodReturnType.ReturnType.BuildAssemblyQualifiedName(builder);
                builder.Append(">");
            }
            else
            {
                elementType = type;
                var types = new List<TypeReference>();
                var declaringType = type;
                while (declaringType != null)
                {
                    types.Add(declaringType);
                    declaringType = declaringType.DeclaringType;
                }

                var baseType = types[types.Count - 1];

                if (!string.IsNullOrEmpty(baseType.Namespace))
                {
                    builder.Append(baseType.Namespace);
                    builder.Append(".");
                }

                builder.Append(baseType.Name);
                for (int i = types.Count - 2; i >= 0; i--)
                {
                    var nestedType = types[i];
                    builder.Append("+").Append(nestedType.Name);
                }

                if (elementType is GenericInstanceType genericInstanceType && genericInstanceType.HasGenericArguments)
                {
                    builder.Append("[");
                    for (var i = 0; i < genericInstanceType.GenericArguments.Count; i++)
                    {
                        var genericArgument = genericInstanceType.GenericArguments[i];
                        if (i > 0)
                        {
                            builder.Append(",");
                        }

                        if (assemblyQualified)
                        {
                            builder.Append("[");
                            genericArgument.BuildAssemblyQualifiedName(builder);
                            builder.Append("]");
                        }
                        else
                        {
                            genericArgument.BuildReflectionFullName(builder, out var _, assemblyQualified: true);
                        }
                    }
                    builder.Append("]");
                }
            }
        }
    }
}
