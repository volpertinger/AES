namespace AES
{
    public class AES
    {
        // ------------------------------------------------------------------------------------------------------------
        // Fields
        // ------------------------------------------------------------------------------------------------------------

        // ------------------------------------------------------------------------------------------------------------
        // Public
        // ------------------------------------------------------------------------------------------------------------
        public static List<byte> GetForwardSBox(int seed)
        {
            List<byte> initial = new();
            for (int i = 0; i <= byte.MaxValue; ++i)
            {
                initial.Add((byte)i);
            }

            List<byte> result = new();
            var random = new Random(seed);
            for (int i = 0; i <= byte.MaxValue; ++i)
            {
                var index = random.Next(initial.Count);
                result.Add(initial[index]);
                initial.RemoveAt(index);
            }

            return result;
        }

        public static List<byte> GetInverseSBox(List<byte> forwardSBox)
        {
            List<byte> result = new byte[byte.MaxValue + 1].ToList();
            for (int i = 0; i <= byte.MaxValue; ++i)
            {
                result[forwardSBox[i]] = (byte)i;
            }
            return result;
        }

        // ------------------------------------------------------------------------------------------------------------
        // Private
        // ------------------------------------------------------------------------------------------------------------
    }
}
