global using static BetterAutocastVPE.Extensions.TranslateExtension;

namespace BetterAutocastVPE.Extensions;

static class TranslateExtension
{
    public static Verse.TaggedString TranslateSafe(
        this string self,
        params Verse.NamedArgument[] args
    )
    {
        if (!Verse.Translator.CanTranslate(self))
        {
            BetterAutocastVPE.DebugError(
                $"Untranslated key: {self}",
                Verse.GenString.GetHashCodeSafe(self)
            );
        }
        return Verse.TranslatorFormattedStringExtensions.Translate(self, args);
    }
}
