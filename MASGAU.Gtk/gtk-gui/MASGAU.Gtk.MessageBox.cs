
// This file has been generated by the GUI designer. Do not modify.
namespace MASGAU.Gtk
{
	public partial class MessageBox
	{
		private global::Gtk.HBox hbox1;
		private global::Gtk.Image icon;
		private global::Gtk.Label messageLabel;
		private global::Gtk.Expander exceptionExpander;
		private global::Gtk.ScrolledWindow GtkScrolledWindow;
		private global::Gtk.TextView exceptionText;
		private global::Gtk.Label exceptionLabel;
		private global::Gtk.Button submitButton;
		private global::Gtk.Button buttonCancel;
		private global::Gtk.Button buttonOk;
		
		protected virtual void Build ()
		{
			global::Stetic.Gui.Initialize (this);
			// Widget MASGAU.Gtk.MessageBox
			this.Name = "MASGAU.Gtk.MessageBox";
			this.WindowPosition = ((global::Gtk.WindowPosition)(4));
			// Internal child MASGAU.Gtk.MessageBox.VBox
			global::Gtk.VBox w1 = this.VBox;
			w1.Name = "dialog1_VBox";
			w1.BorderWidth = ((uint)(2));
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.hbox1 = new global::Gtk.HBox ();
			this.hbox1.Name = "hbox1";
			this.hbox1.Spacing = 6;
			// Container child hbox1.Gtk.Box+BoxChild
			this.icon = new global::Gtk.Image ();
			this.icon.Name = "icon";
			this.hbox1.Add (this.icon);
			global::Gtk.Box.BoxChild w2 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.icon]));
			w2.Position = 0;
			w2.Expand = false;
			w2.Fill = false;
			// Container child hbox1.Gtk.Box+BoxChild
			this.messageLabel = new global::Gtk.Label ();
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("label1");
			this.hbox1.Add (this.messageLabel);
			global::Gtk.Box.BoxChild w3 = ((global::Gtk.Box.BoxChild)(this.hbox1 [this.messageLabel]));
			w3.Position = 1;
			w3.Expand = false;
			w3.Fill = false;
			w1.Add (this.hbox1);
			global::Gtk.Box.BoxChild w4 = ((global::Gtk.Box.BoxChild)(w1 [this.hbox1]));
			w4.Position = 0;
			w4.Expand = false;
			w4.Fill = false;
			// Container child dialog1_VBox.Gtk.Box+BoxChild
			this.exceptionExpander = new global::Gtk.Expander (null);
			this.exceptionExpander.CanFocus = true;
			this.exceptionExpander.Name = "exceptionExpander";
			this.exceptionExpander.Expanded = true;
			// Container child exceptionExpander.Gtk.Container+ContainerChild
			this.GtkScrolledWindow = new global::Gtk.ScrolledWindow ();
			this.GtkScrolledWindow.Name = "GtkScrolledWindow";
			this.GtkScrolledWindow.ShadowType = ((global::Gtk.ShadowType)(1));
			// Container child GtkScrolledWindow.Gtk.Container+ContainerChild
			this.exceptionText = new global::Gtk.TextView ();
			this.exceptionText.CanFocus = true;
			this.exceptionText.Name = "exceptionText";
			this.GtkScrolledWindow.Add (this.exceptionText);
			this.exceptionExpander.Add (this.GtkScrolledWindow);
			this.exceptionLabel = new global::Gtk.Label ();
			this.exceptionLabel.Name = "exceptionLabel";
			this.exceptionLabel.LabelProp = global::Mono.Unix.Catalog.GetString ("ExceptionInformation");
			this.exceptionLabel.UseUnderline = true;
			this.exceptionExpander.LabelWidget = this.exceptionLabel;
			w1.Add (this.exceptionExpander);
			global::Gtk.Box.BoxChild w7 = ((global::Gtk.Box.BoxChild)(w1 [this.exceptionExpander]));
			w7.Position = 1;
			// Internal child MASGAU.Gtk.MessageBox.ActionArea
			global::Gtk.HButtonBox w8 = this.ActionArea;
			w8.Name = "dialog1_ActionArea";
			w8.Spacing = 10;
			w8.BorderWidth = ((uint)(5));
			w8.LayoutStyle = ((global::Gtk.ButtonBoxStyle)(4));
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.submitButton = new global::Gtk.Button ();
			this.submitButton.Sensitive = false;
			this.submitButton.CanFocus = true;
			this.submitButton.Name = "submitButton";
			this.submitButton.UseUnderline = true;
			// Container child submitButton.Gtk.Container+ContainerChild
			global::Gtk.Alignment w9 = new global::Gtk.Alignment (0.5F, 0.5F, 0F, 0F);
			// Container child GtkAlignment.Gtk.Container+ContainerChild
			global::Gtk.HBox w10 = new global::Gtk.HBox ();
			w10.Spacing = 2;
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Image w11 = new global::Gtk.Image ();
			w11.Pixbuf = global::Stetic.IconLoader.LoadIcon (this, "gtk-go-up", global::Gtk.IconSize.Menu);
			w10.Add (w11);
			// Container child GtkHBox.Gtk.Container+ContainerChild
			global::Gtk.Label w13 = new global::Gtk.Label ();
			w13.LabelProp = global::Mono.Unix.Catalog.GetString ("_Submit");
			w13.UseUnderline = true;
			w10.Add (w13);
			w9.Add (w10);
			this.submitButton.Add (w9);
			this.AddActionWidget (this.submitButton, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w17 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w8 [this.submitButton]));
			w17.Expand = false;
			w17.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonCancel = new global::Gtk.Button ();
			this.buttonCancel.CanDefault = true;
			this.buttonCancel.CanFocus = true;
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.UseStock = true;
			this.buttonCancel.UseUnderline = true;
			this.buttonCancel.Label = "gtk-cancel";
			this.AddActionWidget (this.buttonCancel, -6);
			global::Gtk.ButtonBox.ButtonBoxChild w18 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w8 [this.buttonCancel]));
			w18.Position = 1;
			w18.Expand = false;
			w18.Fill = false;
			// Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
			this.buttonOk = new global::Gtk.Button ();
			this.buttonOk.CanDefault = true;
			this.buttonOk.CanFocus = true;
			this.buttonOk.Name = "buttonOk";
			this.buttonOk.UseStock = true;
			this.buttonOk.UseUnderline = true;
			this.buttonOk.Label = "gtk-ok";
			this.AddActionWidget (this.buttonOk, -5);
			global::Gtk.ButtonBox.ButtonBoxChild w19 = ((global::Gtk.ButtonBox.ButtonBoxChild)(w8 [this.buttonOk]));
			w19.Position = 2;
			w19.Expand = false;
			w19.Fill = false;
			if ((this.Child != null)) {
				this.Child.ShowAll ();
			}
			this.DefaultWidth = 400;
			this.DefaultHeight = 300;
			this.Show ();
			this.submitButton.Clicked += new global::System.EventHandler (this.OnSubmitButtonClicked);
		}
	}
}
