using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SwatPresentations
{
	/// <summary>
	/// Interaktionslogik für DlgMessage.xaml
	/// </summary>
	//public partial class DlgMessage : UserControl
	//{
	//	public DlgMessage()
	//	{
	//		InitializeComponent();
	//	}
	//}
	public partial class DlgMessage : Window
	{

		private String _messageHeader;
		private String _messageDetail;
		private MessageLevel _level;
		

		public DlgMessage()
		{
			InitializeComponent();
		}

		private DlgMessage(string messageHeader, String messageDetail,  MessageLevel level)
		{
			InitializeComponent();

			Title = $"Nachricht {System.Diagnostics.Process.GetCurrentProcess().ProcessName}";
			_messageHeader = messageHeader;
			_messageDetail = messageDetail;
			

			_level = level;
			switch (_level)
			{
				case MessageLevel.Error: msgSymbol.Source = new BitmapImage(new Uri(@"/swat;component/resources/Images/error.png", UriKind.Relative)); break;
				case MessageLevel.Warning: msgSymbol.Source = new BitmapImage(new Uri(@"/swat;component/resources/Images/warning.png", UriKind.Relative)); break;
				case MessageLevel.Info: msgSymbol.Source = new BitmapImage(new Uri(@"/swat;component/resources/Images/info.png", UriKind.Relative)); break;
			}
		}

		private void This_Loaded(object sender, RoutedEventArgs e)
		{
			MessageHeader.Text = _messageHeader;
			MessageDetail.Text = _messageDetail;
		}

		public static void Show(string messageHeader, String messageDetail,  MessageLevel level)
		{
			DlgMessage dlg = new DlgMessage(messageHeader, messageDetail,  level);
			dlg.ShowDialog();
		}

		private void CmdClose(object sender, RoutedEventArgs e)
		{
			This.Close();
		}

	}
}
