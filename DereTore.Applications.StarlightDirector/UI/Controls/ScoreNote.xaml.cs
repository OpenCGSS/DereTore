using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;

namespace DereTore.Applications.StarlightDirector.UI.Controls {
    public partial class ScoreNote {

        public ScoreNote() {
            InitializeComponent();
            Radius = DefaultRadius;
            _noteTypeIndicators = new[] { NoteTypeIndicatorSync, NoteTypeIndicatorFlick, NoteTypeIndicatorHold };
        }

        public void UpdateIndicators() {
            var note = Note;
            NoteTypeIndicatorSync.Visibility = note.IsSync ? Visibility.Visible : Visibility.Hidden;
            NoteTypeIndicatorFlick.Visibility = note.IsFlick ? Visibility.Visible : Visibility.Hidden;
            NoteTypeIndicatorHold.Visibility = note.IsHold ? Visibility.Visible : Visibility.Hidden;
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

            var controlWidth = ActualWidth;
            var controlHeight = ActualHeight;
            foreach (var noteTypeIndicator in _noteTypeIndicators) {
                var geometry = (RectangleGeometry)noteTypeIndicator.Clip;
                var rect = geometry.Rect;
                rect.Width = controlWidth / 2;
                rect.Height = controlHeight / 2;
                geometry.Rect = rect;
                var rotateTransform = (RotateTransform)geometry.Transform;
                rotateTransform.CenterX = controlWidth / 2;
                rotateTransform.CenterY = controlHeight / 2;
            }
        }

        private static readonly double DefaultRadius = 15;
        private readonly Ellipse[] _noteTypeIndicators;

    }
}
