using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;
using Meadow.Units;
using System;
using System.Collections.Generic;

namespace Tilt;

public enum Emoji
{
    None,
    Happy,
    Laugh
}

public class DisplayService
{
    public const int BallRadius = 8;
    public const int BallMovement = 2;

    private DisplayScreen _screen;
    private Circle _circle;
    private Picture _emoji;
    private Image? _happy;
    private Image? _laugh;
    private AbsoluteLayout _ballLayout;
    private AbsoluteLayout _powerLayout;
    private Label _instantCurrentLabel;
    private Label _meanCurrentLabel;
    private VerticalBarChart _instantChart;
    private List<float> _currentSeries = new();

    public DisplayService(IPixelDisplay display)
    {
        _screen = new DisplayScreen(display, RotationType._180Degrees);
        for (var i = 0; i < 60; i++)
        {
            _currentSeries.Add(0);
        }

        CreateLayout();
    }

    public bool ShowEmoji(Emoji emoji)
    {
        try
        {
            var image = emoji switch
            {
                Emoji.Happy => _happy ??= Image.LoadFromResource("Tilt.Core.Assets.happy.bmp"),
                Emoji.Laugh => _laugh ??= Image.LoadFromResource("Tilt.Core.Assets.laugh.bmp"),
                _ => null
            };

            if (image != null)
            {
                _emoji.Image = image;
                _emoji.IsVisible = true;
            }
            else
            {
                _emoji.IsVisible = false;
            }
            return true;
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Can't show emoji: {ex.Message}");

            return false;
        }
    }

    public void UpdateMeanCurrent(double milliamps)
    {
        _meanCurrentLabel.Text = $"{milliamps:N0} mA";
    }

    public void UpdateCurrent(Current current)
    {
        _instantCurrentLabel.Text = $"{current.Milliamps:N1} mA";
        _currentSeries.Add((float)current.Milliamps);
        while (_currentSeries.Count > 60)
        {
            _currentSeries.RemoveAt(0);
        }
        _instantChart.Series = _currentSeries.ToArray();

    }

    private void CreateLayout()
    {
        _screen.BackgroundColor = Color.Black;

        _ballLayout = new AbsoluteLayout(_screen, 0, 0, _screen.Width, _screen.Height);
        _emoji = new Picture(_screen.Width / 2 - 25, _screen.Height / 2 - 25, 50, 50, null) { IsVisible = false };
        _circle = new Circle(_screen.Width / 2, _screen.Height / 2, BallRadius) { ForeColor = Color.White };
        _ballLayout.Controls.Add(
            _emoji,
            _circle
            );

        _powerLayout = new AbsoluteLayout(_screen, 0, 0, _screen.Width / 2, _screen.Height);
        _instantCurrentLabel = new Label(0, 0, _screen.Width, 20)
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            VerticalAlignment = VerticalAlignment.Top
        };
        _meanCurrentLabel = new Label(_screen.Width / 2, 0, _screen.Width / 2, 20)
        {
            Font = new Font8x12(),
            TextColor = Color.White,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Top
        };

        _instantChart = new VerticalBarChart(0, 20, _screen.Width, _screen.Height - 20)
        {
            BarSpacing = 0,
            AxisStroke = 1,
            ShowXAxisLabels = false
        };

        _powerLayout.Controls.Add(
            _instantCurrentLabel,
            _meanCurrentLabel,
            _instantChart);

        _ballLayout.IsVisible = true;
        _powerLayout.IsVisible = false;
        _screen.Controls.Add(_ballLayout, _powerLayout);
    }

    public void ToggleView()
    {
        if (_ballLayout.IsVisible)
        {
            ShowPower();
        }
        else
        {
            ShowBall();
        }
    }

    public void ShowBall()
    {
        Resolver.Log.Info($"ShowBall");

        _ballLayout.IsVisible = true;
        _powerLayout.IsVisible = false;
    }

    public void ShowPower()
    {
        Resolver.Log.Info($"ShowPower");

        _ballLayout.IsVisible = false;
        _powerLayout.IsVisible = true;
    }

    public void MoveCircleUp()
    {
        var t = _circle.Top - BallMovement;
        if (t < 0)
        {
            t = 0;
        }
        _circle.Top = t;
    }

    public void MoveCircleDown()
    {
        var t = _circle.Top + BallMovement;
        if (t > _screen.Height - BallRadius * 2)
        {
            t = _screen.Height - BallRadius * 2;
        }
        _circle.Top = t;
    }

    public void MoveCircleLeft()
    {
        var t = _circle.Left - BallMovement;
        if (t < 0)
        {
            t = 0;
        }
        _circle.Left = t;
    }

    public void MoveCircleRight()
    {
        var t = _circle.Left + BallMovement;
        if (t > _screen.Width - 2 * BallRadius)
        {
            t = _screen.Width - 2 * BallRadius;
        }
        _circle.Left = t;
    }
}