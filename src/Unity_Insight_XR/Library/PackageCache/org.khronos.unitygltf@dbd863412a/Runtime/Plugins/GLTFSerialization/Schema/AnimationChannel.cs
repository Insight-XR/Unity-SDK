using Newtonsoft.Json;

namespace GLTF.Schema
{
	/// <summary>
	/// Targets an animation's sampler at a node's property.
	/// </summary>
	public class AnimationChannel : GLTFProperty
	{
		/// <summary>
		/// The index of a sampler in this animation used to compute the value for the
		/// target, e.g., a node's translation, rotation, or scale (TRS).
		/// </summary>
		public AnimationSamplerId Sampler;

		/// <summary>
		/// The index of the node and TRS property to target.
		/// </summary>
		public AnimationChannelTarget Target;

		public AnimationChannel()
		{
		}

		public AnimationChannel(AnimationChannel animationChannel, GLTFRoot root) : base(animationChannel)
		{
			if (animationChannel == null) return;

			Sampler = new AnimationSamplerId(animationChannel.Sampler, root);
			Target = new AnimationChannelTarget(animationChannel.Target, root);
		}

		public static AnimationChannel Deserialize(GLTFRoot root, JsonReader reader, GLTFAnimation anim)
		{
			var animationChannel = new AnimationChannel();

			while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
			{
				var curProp = reader.Value.ToString();

				switch (curProp)
				{
					case "sampler":
						animationChannel.Sampler = AnimationSamplerId.Deserialize(root, anim, reader);
						break;
					case "target":
						animationChannel.Target = AnimationChannelTarget.Deserialize(root, reader);
						break;
					default:
						animationChannel.DefaultPropertyDeserializer(root, reader);
						break;
				}
			}

			return animationChannel;
		}

		public override void Serialize(JsonWriter writer)
		{
			writer.WriteStartObject();

			writer.WritePropertyName("sampler");
			writer.WriteValue(Sampler.Id);

			writer.WritePropertyName("target");
			Target.Serialize(writer);

			base.Serialize(writer);

			writer.WriteEndObject();
		}
	}
}
