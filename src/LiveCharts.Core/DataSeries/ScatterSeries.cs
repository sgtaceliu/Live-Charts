﻿using System;
using System.Drawing;
using LiveCharts.Core.Abstractions;
using LiveCharts.Core.Abstractions.DataSeries;
using LiveCharts.Core.Charts;
using LiveCharts.Core.Coordinates;
using LiveCharts.Core.DataSeries.Data;
using LiveCharts.Core.Drawing;
using LiveCharts.Core.Drawing.Svg;
using LiveCharts.Core.Interaction;
using LiveCharts.Core.ViewModels;

namespace LiveCharts.Core.DataSeries
{
    /// <summary>
    /// The scatter series class.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <seealso cref="CartesianSeries{TModel, PointCoordinate, GeometryPointViewModel, Point}" />
    /// <seealso cref="LiveCharts.Core.Abstractions.DataSeries.IScatterSeries" />
    public class ScatterSeries<TModel>
        : CartesianSeries<TModel, PointCoordinate, GeometryPointViewModel,
                Point<TModel, PointCoordinate, GeometryPointViewModel>>,
            IScatterSeries
    {
        private static ISeriesViewProvider<TModel, PointCoordinate, GeometryPointViewModel> _provider;
        private float _geometrySize;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScatterSeries{TModel}"/> class.
        /// </summary>
        public ScatterSeries()
        {
            GeometrySize = 18;
            StrokeThickness = 1;
            Geometry = Geometry.Circle;
            Charting.BuildFromSettings<IScatterSeries>(this);
        }

        /// <summary>
        /// Gets or sets the size of the <see cref="P:LiveCharts.Core.Abstractions.DataSeries.ISeries.Geometry" /> property.
        /// </summary>
        /// <value>
        /// The size of the geometry.
        /// </value>
        public float GeometrySize
        {
            get => _geometrySize;
            set
            {
                _geometrySize = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc />
        public override Type ResourceKey => typeof(IScatterSeries);

        /// <inheritdoc />
        public override float[] DefaultPointWidth => new[] {0f, 0f};

        /// <inheritdoc />
        public override float[] PointMargin => new[] {GeometrySize, GeometrySize};

        /// <inheritdoc />
        protected override ISeriesViewProvider<TModel, PointCoordinate, GeometryPointViewModel>
            DefaultViewProvider => _provider ??
                                   (_provider = Charting.Current.UiProvider
                                       .GeometryPointViewProvider<TModel, PointCoordinate>());

        /// <inheritdoc />
        public override void UpdateView(ChartModel chart, UpdateContext context)
        {
            var cartesianChart = (CartesianChartModel) chart;
            var x = cartesianChart.Dimensions[0][ScalesAt[0]];
            var y = cartesianChart.Dimensions[1][ScalesAt[1]];

            var uw = chart.Get2DUiUnitWidth(x, y);

            int xi = 0, yi = 1;
            if (chart.InvertXy)
            {
                xi = 1;
                yi = 0;
            }

            Point<TModel, PointCoordinate, GeometryPointViewModel> previous = null;
            foreach (var current in Points)
            {
                var p = new[]
                {
                    chart.ScaleToUi(current.Coordinate[0][0], x),
                    chart.ScaleToUi(current.Coordinate[1][0], y)
                };

                var vm = new GeometryPointViewModel
                {
                    Location = Perform.Sum(new PointF(p[xi], p[yi]), new PointF(uw[0], uw[1])),
                    Diameter = GeometrySize
                };

                if (current.View == null)
                {
                    current.View = ViewProvider.Getter();
                }

                current.ViewModel = vm;
                current.View.DrawShape(current, previous);

                current.InteractionArea = new RectangleInteractionArea(
                    new RectangleF(
                        vm.Location.X - GeometrySize * .5f,
                        vm.Location.Y - GeometrySize * .5f,
                        GeometrySize,
                        GeometrySize));
                previous = current;
            }
        }
    }
}
