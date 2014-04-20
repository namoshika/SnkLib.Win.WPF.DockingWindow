using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace SunokoLibrary.Windows.Controls
{
    public class DockBayLayoutEngine : DockNodeLayoutEngine
    {
        public DockBayLayoutEngine(DockBayBase target)
            : base(target)
        {
            DockPanes = new List<DockPaneLayoutEngine>();
            PaneAdded += pane_PaneAdded;
        }
        public List<DockPaneLayoutEngine> DockPanes { get; protected set; }
        public override IEnumerable<DockPaneLayoutEngine> PaneVisualTree
        {
            get
            {
                var res = Children
                    .Skip(1)
                    .Reverse()
                    .Concat(Children
                        .Where((l, i) => i == 0)
                        .SelectMany(l => l.PaneVisualTree));
                return res;
            }
        }

        public override void Add(DockPaneLayoutEngine pane, DockDirection align, int idx)
        {
            if (pane == null)
                throw new ArgumentNullException("第1引数のneighをnullにする事はできません。");
            if (pane.Owner != null)
                throw new ArgumentException("第1引数のneighは使用中です。");
            if (align == DockDirection.None)
                throw new ArgumentException("第2引数のalignをDockDirection.Noneにすることはできません。");
            if (idx < 0 || idx > Children.Count)
                throw new ArgumentOutOfRangeException("第3引数のinsertIndexが有効範囲外です。");

            if (align != DockDirection.Top)
            {
                var btm = GetChildrenOf(DockDirection.Bottom, Children.Count - idx).FirstOrDefault();
                if (btm != null)
                    foreach (var e in pane.GetChildrenOf(DockDirection.Bottom))
                        e.Bottom = btm.Bottom;
            }
            if (align != DockDirection.Bottom)
            {
                var top = GetChildrenOf(DockDirection.Top, Children.Count - idx).FirstOrDefault();
                if (top != null)
                    foreach (var e in pane.GetChildrenOf(DockDirection.Top))
                        e.Top = top.Top;
            }
            if (align != DockDirection.Left)
            {
                var rgh = GetChildrenOf(DockDirection.Right, Children.Count - idx).FirstOrDefault();
                if (rgh != null)
                    foreach (var e in pane.GetChildrenOf(DockDirection.Right))
                        e.Right = rgh.Right;
            }
            if (align != DockDirection.Right)
            {
                var lft = GetChildrenOf(DockDirection.Left, Children.Count - idx).FirstOrDefault();
                if (lft != null)
                    foreach (var e in pane.GetChildrenOf(DockDirection.Left))
                        e.Left = lft.Left;
            }
            if(Children.Count > 0)
                switch (align)
                {
                    case DockDirection.Top:
                        foreach (var paneInner in pane.GetChildrenOf(~align))
                            paneInner.Bottom = pane;
                        foreach (var thisInner in GetChildrenOf(align, Children.Count - idx))
                            thisInner.Top = pane;
                        break;
                    case DockDirection.Bottom:
                        foreach (var paneInner in pane.GetChildrenOf(~align))
                            paneInner.Top = pane;
                        foreach (var thisInner in GetChildrenOf(align, Children.Count - idx))
                            thisInner.Bottom = pane;
                        break;
                    case DockDirection.Left:
                        foreach (var paneInner in pane.GetChildrenOf(~align))
                            paneInner.Right = pane;
                        foreach (var thisInner in GetChildrenOf(align, Children.Count - idx))
                            thisInner.Left = pane;
                        break;
                    case DockDirection.Right:
                        foreach (var paneInner in pane.GetChildrenOf(~align))
                            paneInner.Left = pane;
                        foreach (var thisInner in GetChildrenOf(align, Children.Count - idx))
                            thisInner.Right = pane;
                        break;
                }
            pane.Initialize(this, this, align);
            base.Add(pane, align, idx);
        }
        public override void Remove(DockPaneLayoutEngine pane)
        {
            if (!Children.Contains(pane))
                throw new ArgumentException("引数paneはこの要素の子要素ではありません。");

            var paneRmvAlign = pane.GetSucceedingDireWhenDeletingPane();

            #region Debug時用状態チェックコード
#if DEBUG
            if (pane.Align == DockDirection.None)
            {
                System.Diagnostics.Debug.Fail(
                    "StatusError", "自身の挿入方向が分かりません。");
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
            if (paneRmvAlign == DockDirection.None)
            {
                System.Diagnostics.Debug.Fail(
                    "StatusError", "自身を消去した後の領域を受け継ぐノードが存在しません。");
                if (System.Diagnostics.Debugger.IsAttached)
                    System.Diagnostics.Debugger.Break();
            }
#endif
            #endregion

            if (pane.Children.Count > 0)
            {
                //子要素を自身の代替要素とする場合、各方向のSplitPane情報の更新が必要。
                //よって自身をSplitPaneとして参照する要素のSplitPaneを子要素へ更新する。
                var newParent = pane.Children.Last();
                switch (pane.Align)
                {
                    case DockDirection.Top:
                        foreach (var p in pane.GetChildrenOf(~pane.Align))
                            p.Bottom = newParent;

                        //BayではChildrenの順番がPaneの逆であると同時に要素が1多い。その
                        //ため、Childrenの要素数からindexを引く事でPane時と同じ順番にし、
                        //Pane.Removeでやってるidx+1はしない事で同じ入力をする
                        foreach (var p in GetChildrenOf(pane.Align, Children.Count - Children.IndexOf(pane)))
                            p.Top = newParent;
                        break;
                    case DockDirection.Bottom:
                        foreach (var p in pane.GetChildrenOf(~pane.Align))
                            p.Top = newParent;

                        //※case DockDirection.Topと同様の注意事項
                        foreach (var p in GetChildrenOf(pane.Align, Children.Count - Children.IndexOf(pane)))
                            p.Bottom = newParent;
                        break;
                    case DockDirection.Left:
                        foreach (var p in pane.GetChildrenOf(~pane.Align))
                            p.Right = newParent;

                        //※case DockDirection.Topと同様の注意事項
                        foreach (var p in GetChildrenOf(pane.Align, Children.Count - Children.IndexOf(pane)))
                            p.Left = newParent;
                        break;
                    case DockDirection.Right:
                        foreach (var p in pane.GetChildrenOf(~pane.Align))
                            p.Left = newParent;

                        //※case DockDirection.Topと同様の注意事項
                        foreach (var p in GetChildrenOf(pane.Align, Children.Count - Children.IndexOf(pane)))
                            p.Right = newParent;
                        break;
                }
                //代替用子要素が持つ、自身と消去要素(paneの事)が接触している部分の
                //SplitPane情報を消去要素が同じ方向に持っているSplitPane情報に差し替え
                switch (paneRmvAlign)
                {
                    case DockDirection.Top:
                        newParent.Bottom = pane.Bottom;
                        break;
                    case DockDirection.Bottom:
                        newParent.Top = pane.Top;
                        break;
                    case DockDirection.Left:
                        newParent.Right = pane.Right;
                        break;
                    case DockDirection.Right:
                        newParent.Left = pane.Left;
                        break;
                }
            }
            else if (Children.IndexOf(pane) == 0 && Children.Count > 1)
            {
                //Bayで最初に挿入した子要素は全方向に広がり、Splitterを持たない。そのため、
                //他要素にSplitPaneで参照されることはない。
                //この条件によってBayの最初要素が削除される時に代替要素となる2番目は他要素
                //からのSplitPane参照を消す必要がある。
                var newFillPane = Children[1];
                foreach (var item in newFillPane.GetChildrenOf(~newFillPane.Align))
                    switch (newFillPane.Align)
                    {
                        case DockDirection.Top:
                            item.Bottom = null;
                            break;
                        case DockDirection.Bottom:
                            item.Top = null;
                            break;
                        case DockDirection.Left:
                            item.Right = null;
                            break;
                        case DockDirection.Right:
                            item.Left = null;
                            break;
                    }
            }
            else
            {
                //最初要素にも新親要素にもならない独身要素が消える際には削除方向にあるpaneの
                //splitPane情報の自身を参照する方向を自身をまたいだ方向にあるpaneに差し替える
                foreach (var pLayoutEngine in GetChildrenOf(paneRmvAlign, Children.Count - Children.IndexOf(pane)))
                    switch (paneRmvAlign)
                    {
                        case DockDirection.Top:
                            pLayoutEngine.Bottom = pane.Bottom;
                            break;
                        case DockDirection.Bottom:
                            pLayoutEngine.Top = pane.Top;
                            break;
                        case DockDirection.Left:
                            pLayoutEngine.Right = pane.Right;
                            break;
                        case DockDirection.Right:
                            pLayoutEngine.Left = pane.Left;
                            break;
                    }
            }

            base.Remove(pane);
        }
        public override DockBaseConfig GetConfig()
        {
            var conf = new DockBayBaseConfig();
            InitConfig(conf);
            return conf;
        }
        protected override void InitConfig(DockBaseConfig config)
        {
            var conf = (DockBayBaseConfig)config;
            conf.ChildCount = OwnNodes.Count;
            Target.InternalInitConfig(conf);

            base.InitConfig(config);
        }
        internal override IEnumerable<DockPaneLayoutEngine> GetChildrenOf(DockDirection align, int skip = 0, bool deep = true)
        {
            var flg = DockDirection.None;
            var frstIdx = Children.Count - skip - 1;
            var enumer = Children
                .Reverse<DockPaneLayoutEngine>()
                .Skip(skip)
                .Where(n =>
                    {
                        //初回実行にはflgを初期化
                        if (Children[frstIdx] == n)
                            flg = DockDirection.None;
                        return n.Align != ~align || n == Children.FirstOrDefault();
                    })
                .TakeWhile(n =>
                    {
                        var tmp = flg != align;
                        flg = n.Align;
                        return tmp;
                    });
            if (deep)
                enumer = enumer.SelectMany(n => n.GetChildrenOf(align));
            return enumer;
        }

        void pane_PaneAdded(object sender, LayoutEngineAddedEventArgs e)
        {
            var pane = e.DockPane;
            pane.PaneAdded += pane_PaneAdded;
            pane.Removed += pane_Removed;

            DockPanes.Add(e.DockPane);
            OnPaneAddedInBay(new LayoutEngineAddedEventArgs((DockNodeLayoutEngine)sender, e.DockPane, e.Align, e.InsertIndex));
        }
        void pane_Removed(object sender, EventArgs e)
        {
            var pane = sender as DockPaneLayoutEngine;
            pane.PaneAdded -= pane_PaneAdded;
            pane.Removed -= pane_Removed;

            DockPanes.Remove(pane);
            OnPaneRemovedInBay(new LayoutEngineEventArgs(pane));
        }
        protected override void OnPaneAdded(LayoutEngineAddedEventArgs e)
        {
            base.OnPaneAdded(e);
            for (var i = 0; i < Children.Count; i++)
                Children[i].SplitterVisible = i != 0;
        }
        protected override void OnPaneRemoved(LayoutEngineEventArgs e)
        {
            base.OnPaneRemoved(e);
            for (var i = 0; i < Children.Count; i++)
                Children[i].SplitterVisible = i != 0;
        }

        public event LayoutEngineAddedEventHandler PaneAddedInBay;
        protected void OnPaneAddedInBay(LayoutEngineAddedEventArgs e)
        {
            if (PaneAddedInBay != null)
                PaneAddedInBay(this, e);
        }
        public event LayoutEngineEventHandler PaneRemovedInBay;
        protected void OnPaneRemovedInBay(LayoutEngineEventArgs e)
        {
            if (PaneRemovedInBay != null)
                PaneRemovedInBay(this, e);
        }
    }
}