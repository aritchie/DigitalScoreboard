# Digital Scoreboard


## TODO

* Online Error Log - AppCenter, Firebase, Sentry, etc
* Yards-to-go for 1st down
* Timeouts Remaining
* Game Clock needs "total minutes:seconds"
* Better design

## Ideas

* Half time/break fullscreen timer?
* Full Screen
    * Play Clock
    * Game Clock
    * Score


<ContentPage.Resources>
        <ResourceDictionary>
            <Style TargetType="VerticalStackLayout">
                <Setter Property="VerticalOptions" Value="FillAndExpand" />
                <Setter Property="HorizontalOptions" Value="FillAndExpand" />
            </Style>

            <Style x:Key="Main" TargetType="Border">
                <Setter Property="Stroke" Value="White" />
                <Setter Property="StrokeThickness" Value="10" />
            </Style>
            <!--<Style TargetType="Frame">
                <Setter Property="BackgroundColor" Value="Black" />
                <Setter Property="BorderColor" Value="White" />
                <Setter Property="CornerRadius" Value="10" />
            </Style>-->
            <Style x:Key="Title" TargetType="Label">
                <Setter Property="FontSize" Value="70" />
                <Setter Property="TextColor" Value="White" />
                <Setter Property="FontFamily" Value="{Binding Font}" />
                <Setter Property="HorizontalTextAlignment" Value="Center" />
            </Style>

            <Style x:Key="Info" TargetType="Label">
                <Setter Property="FontSize" Value="88" />
                <Setter Property="TextColor" Value="Yellow" />
                <Setter Property="FontFamily" Value="{Binding Font}" />
                <Setter Property="HorizontalTextAlignment" Value="Center" />
            </Style>
        </ResourceDictionary>
    </ContentPage.Resources>

    <ContentPage.Content>
        <Border Margin="0" Style="{StaticResource Main}">

            <Grid RowDefinitions="1*, 1*" ColumnDefinitions="1*, 2*, 1*">

                <VerticalStackLayout Grid.Row="0" Grid.Column="0">
                    <!--
                    <TapGestureRecognizer Command="{Binding SetHomeScore}" />
                    <Label Text="{Binding HomeTeamName}" />
                    <Label Text="{Binding HomeTeamScore}" />
                    -->
                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="0" Grid.Column="1">
                    <Border Style="{StaticResource Main}" >
                        <!--
                        <TapGestureRecognizer Command="{Binding TogglePeriodClock}" />

                        <Label Text="{Binding PeriodClock, StringFormat='{0:c}'}" />
                        -->
                    </Border>


                    <!--
                    <TapGestureRecognizer Command="{Binding IncrementPeriod}" />
                    <Label Text="{Binding Period}" />
                    -->
                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="0" Grid.Column="2">
                    <!--
                    <TapGestureRecognizer Command="{Binding SetAwayScore}" />
 
                    <Label Text="{Binding AwayTeamName}" />

                    <Label Text="{Binding AwayTeamScore}" />
                    -->
                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="1" Grid.Column="0">
                    <!--
                    <TapGestureRecognizer Command="{Binding IncrementDown}" />
                    <Label Text="{Binding Down}" />
                    -->

                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="1" Grid.Column="1">
                    <!--yards to go-->
                    <!--ball on the X-->
                </VerticalStackLayout>

                <VerticalStackLayout Grid.Row="1" Grid.Column="2">
                    <!--
                    <TapGestureRecognizer Command="{Binding TogglePlayClock}" />
                    <Label Text="{Binding PlayClock}" />
                    -->
                </VerticalStackLayout>
            </Grid>
        </Border>
    </ContentPage.Content>