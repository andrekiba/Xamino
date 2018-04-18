using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Plugin.Connectivity;
using SocketLite.Services;
using SocketLite.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamino.Base;
using System.Linq;
using System.Net.Sockets;
using System.IO;

namespace Xamino.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CommandPage : ContentPage
	{
        #region Fields

        private TcpSocketListener tcpListener;

        #endregion

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
            #region TCP listener

            //var communicationInterface = new CommunicationsInterface();
            //var allInterfaces = communicationInterface.GetAllInterfaces();
            //var networkInterface = allInterfaces.FirstOrDefault(x => x.IpAddress == Address);

            //tcpListener = new TcpSocketListener();

            //var obseravbleListener = await tcpListener.CreateObservableListener(80, networkInterface, false);

            //var subscriberTcpListener = obseravbleListener.Subscribe(
                //client =>
                //{
                //    //Insert your code here
                //},
                //ex =>
                //{
                //    // Insert your exception code here
                //    Console.WriteLine("Error!");
                //},
                //() =>
                //{
                //    // Insert your completed code here
                //    Console.WriteLine("End!");
                //});

            #endregion

            var command = $"{Command}\n";
	        var tcpClient = new TcpSocketClient();
	        var ok = false;

	        using (var sending = UserDialogs.Instance.Loading("Invio..."))
	        {
	            ok = await AsyncHelper.DoFunc(async () =>
	            {
	                await tcpClient.ConnectAsync(Address, Port);
	                var bytes = Encoding.ASCII.GetBytes(command);
	                await tcpClient.WriteStream.WriteAsync(bytes, 0, bytes.Length);
                }, "Problema invio comando");
            }

	        //se l'invio non è andato a buon fine non aspetto la risposta
            if (!ok)
	            return false;

            //do tempo ad arduino di scrivere la risposta
            await Task.Delay(TimeSpan.FromSeconds(3));

            //verifico di essere ancora connesso
	        if (tcpClient.IsConnected)
	        {
	            using (var receiving = UserDialogs.Instance.Loading("Ricezione..."))
	            {
	                var cts = new CancellationTokenSource();
	                cts.CancelAfter(TimeSpan.FromSeconds(2));

	                ok = await AsyncHelper.DoFunc(async () =>
	                {
                        if (tcpClient.ReadStream.CanRead)
                        {
                            var bytes = await ((NetworkStream)tcpClient.ReadStream).ReadFullyAsync(cts.Token);
                            Response = Encoding.ASCII.GetString(bytes);
                        }
	                }, "Problema in ricezione", "Ricevuto!");
	            }

	            tcpClient.Disconnect();
	        }
	        else
	            ok = false;

	        return ok;
	    }
    }
}