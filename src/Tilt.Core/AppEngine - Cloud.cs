using Meadow;
using Meadow.Cloud;

namespace Tilt;

public class SetEmojiCommand : IMeadowCommand
{
    public int Value { get; set; }
}

public partial class AppEngine
{
    public SetEmojiCommand Command { get; set; }

    private void InitializeCloud()
    {
        Resolver.MeadowCloudService.SendEvent(
            new CloudEvent
            {
                Description = $"Tilt Started on {Resolver.Device.Information.Platform}"
            });

        Resolver.CommandService.Subscribe<SetEmojiCommand>(c =>
        {
            Resolver.Log.Info($"Command received! {c.Value}");

            var _ = c.Value switch
            {
                > 100 => _displayService.ShowEmoji(Emoji.Laugh),
                > 50 => _displayService.ShowEmoji(Emoji.Happy),
                _ => _displayService.ShowEmoji(Emoji.None)
            };
        });
    }
}
