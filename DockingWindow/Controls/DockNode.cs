using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace SunokoLibrary.Windows.Controls
{
    public abstract class DockNode : ItemsControl
    {
        protected abstract DockNodeLayoutEngine LayoutEngine { get; }
        internal abstract Size ItemsPanelSize { get; }
        internal bool IsReconstruction { get; set; }

        public void AddPanesOf(DockBayBase bay, DockDirection align, DockLength size)
        {
            var bayConf = bay.GetConfig();
            var pneConf = bayConf.Children.FirstOrDefault();
            foreach (var p in ((DockBayLayoutEngine)bay.LayoutEngine).DockPanes
                .Select(l => (DockPaneBase)l.Target).ToArray())
                p.Remove();

            var pne = (DockPaneBase)pneConf.Target.Target;
            pne.Length = size;
            pne.Align = align;
            Items.Add(pne);
            foreach (var pConf in bayConf.Children
                .Skip(1).Reverse()
                .Concat(pneConf.Children))
            {
                var pane = (DockPaneBase)pConf.Target.Target;
                pane.Align = pConf.Align;
                pane.Length = pConf.Size;
                pneConf.Target.Target.Items.Add(pane);
                pane.ApplyConfig(pConf);
            }
        }
        public DockBaseConfig GetConfig()
        { return LayoutEngine.GetConfig(); }
        public void ApplyConfig(DockBaseConfig config)
        { LayoutEngine.SetConfig(config); }
        internal abstract void InternalApplyConfig(DockBaseConfig config);
        internal abstract void InternalInitConfig(DockBaseConfig config);

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (!IsReconstruction)
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Reset:
                        for (var i = 0; i < Items.Count; i++)
                        {
                            var pane = (DockPaneBase)Items[i];
                            LayoutEngine.Add((DockPaneLayoutEngine)pane.LayoutEngine, pane.Align, i);
                        }
                        break;
                    case NotifyCollectionChangedAction.Add:
                        for (var i = 0; i < e.NewItems.Count; i++)
                        {
                            var pane = (DockPaneBase)e.NewItems[i];
                            LayoutEngine.Add((DockPaneLayoutEngine)pane.LayoutEngine, pane.Align, e.NewStartingIndex + i);
                        }
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        for (int i = 0; i < e.OldItems.Count; i++)
                        {
                            var pane = (DockPaneBase)e.OldItems[i];
                            if (pane.Items.Count > 0)
                            {
                                //paneの子要素が消えないようにする
                                var newParent = (DockPaneBase)pane.Items[pane.Items.Count - 1];
                                IsReconstruction = true;
                                pane.IsReconstruction = true;
                                newParent.IsReconstruction = true;

                                pane.Items.Remove(newParent);
                                Items.Insert(e.OldStartingIndex + i, newParent);
                                for (var j = 0; pane.Items.Count > 0; j++)
                                {
                                    var child = (DockPaneBase)pane.Items[0];
                                    pane.Items.Remove(child);
                                    newParent.Items.Insert(j, child);
                                }

                                IsReconstruction = false;
                                pane.IsReconstruction = false;
                                newParent.IsReconstruction = false;
                            }
                            LayoutEngine.Remove((DockPaneLayoutEngine)pane.LayoutEngine);
                        }
                        break;
                    case NotifyCollectionChangedAction.Move:
                    case NotifyCollectionChangedAction.Replace:
                        throw new NotSupportedException(
                            string.Format("{0}はItemsのMove, Replaceには未対応です。", GetType().Name));
                }
        }
    }
    [System.ComponentModel.TypeConverter(typeof(DockLengthTypeConverter))]
    public class DockLength
    {
        public DockLength(double rate = 0.25, double pixel = double.PositiveInfinity)
        {
            Rate = rate;
            Pixel = pixel;
        }
        public double Rate { get; set; }
        public double Pixel { get; set; }

        public double GetSize(double baseSize)
        { return Math.Min(baseSize * Rate, Pixel); }
        public override bool Equals(object obj)
        {
            if (obj is DockLength)
            {
                var param = (DockLength)obj;
                return param.Pixel == Pixel && param.Rate == Rate;
            }
            else
                return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public class DockLengthTypeConverter : System.ComponentModel.TypeConverter
    {
        public override bool CanConvertTo(
            System.ComponentModel.ITypeDescriptorContext context, Type destinationType)
        { return destinationType == typeof(string); }
        public override bool CanConvertFrom(
            System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        { return sourceType == typeof(string); }
        public override object ConvertFrom(
            System.ComponentModel.ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture, object value)
        {
            var regex = System.Text.RegularExpressions.Regex.Match(
                (string)value, @"(?<rate>\d+(?:.\d+)?)(?:\((?<pixel>\d+)\))?");
            if (!regex.Success)
                return null;

            try
            {
                if (!regex.Groups["rate"].Success)
                    return null;

                var rate = double.Parse(regex.Groups["rate"].Value);
                var pixel = regex.Groups["pixel"].Success ?
                    double.Parse(regex.Groups["pixel"].Value) : double.PositiveInfinity;
                return new DockLength(rate, pixel);
            }
            catch
            { return null; }
        }
        public override object ConvertTo(
            System.ComponentModel.ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            var dkLen = (DockLength)value;
            var pixel = dkLen.Pixel >= 0 && dkLen.Pixel <= double.MaxValue ? dkLen.Pixel : double.NaN;
            var rate = dkLen.Rate >= 0 && dkLen.Rate <= 1 ? dkLen.Rate : double.NaN;
            if (double.IsNaN(rate))
                if (double.IsNaN(pixel))
                    return string.Format("{0}({1})", rate, pixel);
                else
                    return string.Format("{0}", rate);
            else
                return null;
        }
    }
    public class DockBaseConfig
    {
        [XmlArrayItem]
        public DockPaneBaseConfig[] Children { get; set; }
    }
}
