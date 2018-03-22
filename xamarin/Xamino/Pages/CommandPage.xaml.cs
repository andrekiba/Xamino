using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Plugin.Connectivity;
using SocketLite.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamino.Base;

namespace Xamino.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CommandPage : ContentPage
	{
		public CommandPage ()
		{
			InitializeComponent ();
		}

        #region Properties

	    public string Address { get; set; }
	    public string Port { get; set; }
	    public string Command { get; set; }
	    public string Response { get; set; }

        #endregion

        #region Commands

	    //private ICommand sendCommand;
	    //public ICommand SendCommand => sendCommand ?? (sendCommand = new Command(
	    //async () =>
	    //{
	    //    var command = $"{Command}\n";
	    //    await DisplayAlert("Command", "Comando inviato", "OK");
	    //},
	    //CanSendCommand));

        #endregion

        #region Methods

	    private bool CanSendCommand()
	    {
	        return Address.IsIpAddress() &&
	               Port.IsPort() &&
	               !Command.IsNullOrWhiteSpace();
	    }

	    #endregion

	    private async void SendButtonOnClicked(object sender, EventArgs e)
	    {
	        if (!CanSendCommand())
	        {
	            await UserDialogs.Instance.AlertAsync("Compila correttamente i campi!", "Errore", "OK");
                return;
	        }

	        if (!CrossConnectivity.Current.IsConnected)
	        {
	            await UserDialogs.Instance.AlertAsync("Sei offline!", "Errore", "OK");
	            return;
	        }

	        if (await ExecSendCommand())
	        {
	            Command = string.Empty;
            }	        
	    }

	    private async Task<bool> ExecSendCommand()
	    {
	        var command = $"{Command}\n";
	        var tcpClient = new TcpSocketClient();
	        var result = false;

	        using (var progress = UserDialogs.Instance.Loading("Invio..."))
	        {
	            result = await AsyncHelper.DoAction(async () =>
	            {
	                //await tcpClient.ConnectAsync(Address, Port);
	                //var bytes = Encoding.ASCII.GetBytes(command);
	                //await tcpClient.WriteStream.WriteAsync(bytes, 0, bytes.Length);
	                await Task.Delay(TimeSpan.FromSeconds(3));
	                progress.Title = "Ricezione...";
	                await Task.Delay(TimeSpan.FromSeconds(3));

                }, "Problema invio comando");
            }

	        if (!result)
	            return false;

	        using (var receiving = UserDialogs.Instance.Loading("Ricezione..."))
	        {
	            //if (tcpClient.IsConnected)
	            //{
	            //    var cts = new CancellationTokenSource();
	            //    cts.CancelAfter(TimeSpan.FromSeconds(10));

	            //    var readBuffer = new byte[tcpClient.ReadStream.Length];

	            //    await tcpClient.ReadStream.ReadAsync(readBuffer, 0, (int)tcpClient.ReadStream.Length, cts.Token);

	            //    Response = Encoding.ASCII.GetString(readBuffer);
	            //}

	            //tcpClient.Disconnect();

	            result = await AsyncHelper.DoAction(async () =>
	            {
	                await Task.Delay(TimeSpan.FromSeconds(3));
	            }, "Problema in ricezione", "Ricevuto!");
            }

	        if (result)
	            Response = "CIAO!";

	        return result;
	    }
    }
}