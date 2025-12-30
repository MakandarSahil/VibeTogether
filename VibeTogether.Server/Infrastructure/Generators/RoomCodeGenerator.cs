using NanoidDotNet;

namespace VibeTogether.Server.Infrastructure.Generators
{
    public static class RoomCodeGenerator
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int CodeLength = 6;

        public static string Generate()
        {
            return Nanoid.Generate(
                alphabet: Alphabet,
                size: CodeLength
            );
        }
    }
}
