namespace QComp
{
    [Command(PackageIds.MyCommand)]
    internal sealed class QCompWindowCommand : BaseCommand<QCompWindowCommand>
    {
        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return QCompWindow.ShowAsync();
        }
    }
}
