namespace Nager.AmazonEc2.InstallScript
{
    public interface IInstallScript
    {
        bool Add(string command);
        string Create();
    }
}