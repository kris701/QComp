using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace QComp
{
    public class QCompWindow : BaseToolWindow<QCompWindow>
    {
        public override string GetTitle(int toolWindowId) => "QComp Tool Window";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new QCompWindowControl());
        }

        [Guid("dc4f1d4b-4f4a-4c8b-ba9f-53c6ed065275")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}