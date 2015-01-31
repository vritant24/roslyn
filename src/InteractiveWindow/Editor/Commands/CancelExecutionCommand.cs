#if TODO
using System.ComponentModel.Composition;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Roslyn.Editor.InteractiveWindow {
    [Export(typeof(IInteractiveWindowCommand))]
    internal sealed class CancelExecutionCommand : InteractiveWindowCommand {
        public override Task<ExecutionResult> Execute(IInteractiveWindow window, string arguments) {
            window.AbortCommand();
            return ExecutionResult.Succeeded;
        }

        public override string Description {
            get { return "Stops execution of the current command."; }
        }

        public override object ButtonContent {
            get {
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microsoft.VisualStudio.Resources.CancelEvaluation.gif");
                image.EndInit();
                var res = new Image();
                res.Source = image;
                res.Width = res.Height = 16;
                return res;
            }
        }
    }
}
#endif