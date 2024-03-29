﻿namespace DigitalScoreboard;


public partial class BoardLabel : ContentView
{
	public BoardLabel()
	{
		this.InitializeComponent();
	}


    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(BoardLabel),
        null
    );
    public string Title
    {
        get => (string)this.GetValue(TitleProperty);
        set => this.SetValue(TitleProperty, value);
    }


    public static readonly BindableProperty InfoProperty = BindableProperty.Create(
        nameof(Info),
        typeof(string),
        typeof(BoardLabel),
        null
    );
    public string Info
    {
        get => (string)this.GetValue(InfoProperty);
        set => this.SetValue(InfoProperty, value);
    }


    public static readonly BindableProperty IsInfoFramedProperty = BindableProperty.Create(
        nameof(IsInfoFramed),
        typeof(bool),
        typeof(BoardLabel),
        null
    );
    public bool IsInfoFramed
    {
        get => (bool)this.GetValue(InfoProperty);
        set => this.SetValue(InfoProperty, value);
    }

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(BoardLabel),
        null
    );
    public ICommand Command
    {
        get => (ICommand)this.GetValue(CommandProperty);
        set => this.SetValue(CommandProperty, value);
    }


    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(BoardLabel),
        null
    );
    public object CommandParameter
    {
        get => this.GetValue(CommandParameterProperty);
        set => this.SetValue(CommandParameterProperty, value);
    }


    protected override void OnPropertyChanged(string? propertyName = null)
    {
        if (propertyName == TitleProperty.PropertyName)
        {
            this.lblTitle.Text = this.Title;
            this.lblTitle.IsVisible = !String.IsNullOrWhiteSpace(this.Title);
        }
        else if (propertyName == InfoProperty.PropertyName)
        {
            this.lblInfo.Text = this.Info;
        }
        else if (propertyName == CommandProperty.PropertyName)
        {
            grCommand.Command = this.Command;
        }
        else if (propertyName == CommandParameterProperty.PropertyName)
        {
            grCommand.CommandParameter = this.CommandParameter;
        }
        else if (propertyName == IsInfoFramedProperty.PropertyName)
        {
            //frInfo.BorderColor = this.IsInfoFramed
            //    ? Color.LightGray
            //    : Color. //Color.Transparent;
        } 
    }
}
