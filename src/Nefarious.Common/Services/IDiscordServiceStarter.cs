namespace Nefarious.Common.Services;

public interface IDiscordServiceStarter
{
    Task StopClient();
    Task StartClient();
}