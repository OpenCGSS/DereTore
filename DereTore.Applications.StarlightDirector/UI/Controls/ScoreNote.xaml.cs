using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    public partial class ScoreNote {

        public ScoreNote() {
            InitializeComponent();
            //Fill = Circle.Fill;
            //Stroke = Circle.Stroke;
            Radius = DefaultRadius;
        }

        private Brush GetBorderBrush() {
            return GetBorderBrush(Party);
        }

        private Brush GetFillBrush() {
            return GetFillBrush(Party);
        }

        private Brush GetBorderBrush(Party party) {
            var app = Application.Current;
            if (IsSelected) {
                return Brushes.LawnGreen;
            }
            switch (party) {
                case Party.Neutral:
                    return app.FindResource<Brush>(App.ResourceKeys.NeutralStrokeBrush);
                case Party.Cute:
                    return app.FindResource<Brush>(App.ResourceKeys.CuteStrokeBrush);
                case Party.Cool:
                    return app.FindResource<Brush>(App.ResourceKeys.CoolStrokeBrush);
                case Party.Passion:
                    return app.FindResource<Brush>(App.ResourceKeys.PassionStrokeBrush);
                default:
                    throw new ArgumentOutOfRangeException(nameof(party), party, null);
            }
        }

        private Brush GetFillBrush(Party party) {
            var app = Application.Current;
            switch (party) {
                case Party.Neutral:
                    return app.FindResource<Brush>(App.ResourceKeys.NeutralFillBrush);
                case Party.Cute:
                    return app.FindResource<Brush>(App.ResourceKeys.CuteFillBrush);
                case Party.Cool:
                    return app.FindResource<Brush>(App.ResourceKeys.CoolFillBrush);
                case Party.Passion:
                    return app.FindResource<Brush>(App.ResourceKeys.PassionFillBrush);
                default:
                    throw new ArgumentOutOfRangeException(nameof(party), party, null);
            }
        }

        private void ScoreNote_OnSizeChanged(object sender, SizeChangedEventArgs e) {
            var clip = ImageClipper.Clip as EllipseGeometry;
            var width = ImageClipper.ActualWidth;
            var height = ImageClipper.ActualHeight;
            var margin = ImageClipper.Margin;
            Debug.Assert(clip != null, "clip != null");
            clip.Center = new Point((width - margin.Left - margin.Right) / 2 + margin.Left, (height - margin.Top - margin.Bottom) / 2 + margin.Top);
            clip.RadiusX = width / 2;
            clip.RadiusY = height / 2;

            clip = CircleInnerGlow.Clip as EllipseGeometry;
            width = CircleInnerGlow.ActualWidth;
            height = CircleInnerGlow.ActualHeight;
            margin = CircleInnerGlow.Margin;
            Debug.Assert(clip != null, "clip != null");
            clip.Center = new Point((width - margin.Left - margin.Right) / 2 + margin.Left, (height - margin.Top - margin.Bottom) / 2 + margin.Top);
            clip.RadiusX = width / 2;
            clip.RadiusY = height / 2;
        }

        private static readonly double DefaultRadius = 15;

    }
}
