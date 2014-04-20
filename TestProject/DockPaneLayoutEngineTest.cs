using SunokoLibrary.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TestProject
{
    /// <summary>
    ///DockPaneLayoutEngineTest のテスト クラスです。すべての
    ///DockPaneLayoutEngineTest 単体テストをここに含めます
    ///</summary>
    [TestClass()]
    public class DockPaneLayoutEngineTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///現在のテストの実行についての情報および機能を
        ///提供するテスト コンテキストを取得または設定します。
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region 追加のテスト属性
        // 
        //テストを作成するときに、次の追加属性を使用することができます:
        //
        //クラスの最初のテストを実行する前にコードを実行するには、ClassInitialize を使用
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //クラスのすべてのテストを実行した後にコードを実行するには、ClassCleanup を使用
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //各テストを実行する前にコードを実行するには、TestInitialize を使用
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //各テストを実行した後にコードを実行するには、TestCleanup を使用
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        /// <summary>
        ///DockPaneLayoutEngine コンストラクター のテスト
        ///</summary>
        [TestMethod()]
        public void DockPaneLayoutEngineConstructorTest()
        {
            var target = new DockPaneLayoutEngine(new DockPaneBase());
            Assert.AreEqual(DockDirection.None, target.Align);
            Assert.IsNull(target.Parent);
            Assert.IsNull(target.Owner);
            Assert.IsNotNull(target.Children);
            Assert.AreEqual(0, target.Children.Count);
        }

        /// <summary>
        ///Add のテスト
        ///</summary>
        [TestMethod()]
        public void AddTest()
        {
            var target = new DockPaneLayoutEngine(new DockPaneBase());

            //AddTest
            //DockPaneNeighs
            var tmp = new[]
                {
                    new { LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Left, Index = 0, Count = 1 },
                    new { LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Top, Index = 1, Count = 2 },
                    new { LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Right, Index = 2, Count = 3 },
                    new { LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Bottom, Index = 3, Count = 4 },
                    new { LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Right, Index = 3, Count = 5 },
                }
                .ToArray();
            //TestDatas
            var testDtIdx = 0;
            var testDt = new[]
                {
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = tmp[0].LayoutEngine },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = tmp[1].LayoutEngine, Left = tmp[0].LayoutEngine, Right = (DockPaneLayoutEngine)null },
                    new { Top = tmp[1].LayoutEngine, Bottom = (DockPaneLayoutEngine)null, Left = tmp[2].LayoutEngine, Right = (DockPaneLayoutEngine)null },
                    new { Top = tmp[3].LayoutEngine, Bottom = (DockPaneLayoutEngine)null, Left = tmp[0].LayoutEngine, Right = tmp[2].LayoutEngine },
                    new { Top = tmp[1].LayoutEngine, Bottom = (DockPaneLayoutEngine)null, Left = tmp[4].LayoutEngine, Right = tmp[2].LayoutEngine },
                };
            //MargeCollection
            var children = tmp
                .Select(n => new
                {
                    Align = n.Align,
                    LayoutEngine = n.LayoutEngine,
                    TestData = testDt[testDtIdx++],
                    Index = n.Index,
                    Count = n.Count,
                })
                .ToArray();
            foreach (var item in children)
            {
                target.Add(item.LayoutEngine, item.Align, item.Index);

                Assert.AreEqual(target, item.LayoutEngine.Parent);
                Assert.AreEqual(item.Align, item.LayoutEngine.Align);
                Assert.AreEqual(target, item.LayoutEngine.Parent);
                Assert.IsNull(item.LayoutEngine.Owner);
                Assert.AreEqual(item.Index, target.Children.IndexOf(item.LayoutEngine));
                Assert.AreEqual(item.Count, target.Children.Count);

                Assert.AreEqual(item.TestData.Top, item.LayoutEngine.Top);
                Assert.AreEqual(item.TestData.Bottom, item.LayoutEngine.Bottom);
                Assert.AreEqual(item.TestData.Left, item.LayoutEngine.Left);
                Assert.AreEqual(item.TestData.Right, item.LayoutEngine.Right);
                switch (item.Align)
                {
                    case DockDirection.Top:
                        Assert.AreEqual(0,
                            target.GetChildrenOf(item.Align, target.Children.IndexOf(item.LayoutEngine) + 1)
                            .Select(n => n.Top)
                            .Where(n => n != item.LayoutEngine)
                            .Count());
                        break;
                    case DockDirection.Bottom:
                        Assert.AreEqual(0,
                            target.GetChildrenOf(item.Align, target.Children.IndexOf(item.LayoutEngine) + 1)
                            .Select(n => n.Bottom)
                            .Where(n => n != item.LayoutEngine)
                            .Count());
                        break;
                    case DockDirection.Left:
                        Assert.AreEqual(0,
                            target.GetChildrenOf(item.Align, target.Children.IndexOf(item.LayoutEngine) + 1)
                            .Select(n => n.Left)
                            .Where(n => n != item.LayoutEngine)
                            .Count());
                        break;
                    case DockDirection.Right:
                        Assert.AreEqual(0,
                            target.GetChildrenOf(item.Align, target.Children.IndexOf(item.LayoutEngine) + 1)
                            .Select(n => n.Right)
                            .Where(n => n != item.LayoutEngine)
                            .Count());
                        break;
                }
            }

            //InsertTest
            var lyout = new DockPaneLayoutEngine(new DockPaneBase());
            target.Add(lyout, DockDirection.Left, 3);
            Assert.AreEqual(tmp[1].LayoutEngine, lyout.Top);
            Assert.AreEqual(tmp[0].LayoutEngine, lyout.Left);
            Assert.AreEqual(lyout, lyout.Right);
            Assert.AreEqual(null, lyout.Bottom);
            Assert.AreEqual(target.Left, lyout);
            Assert.AreEqual(tmp[3].LayoutEngine.Left, lyout);
            Assert.AreEqual(target, lyout.Parent);

            var bay = new DockBayLayoutEngine(new DockBayBase());
            bay.Add(target, DockDirection.Top);
            foreach (var item in children)
                Assert.AreEqual(bay, item.LayoutEngine.Owner);

            //不正引数用
            var exCnter = 0;
            try { target.Add((DockPaneLayoutEngine)null, DockDirection.Top, 0); }
            catch (ArgumentNullException) { exCnter++; }
            catch { }

            try { target.Add(children[0].LayoutEngine, DockDirection.Bottom, 0); }
            catch (ArgumentException) { exCnter++; }
            catch { }

            try { target.Add(new DockPaneLayoutEngine(new DockPaneBase()), DockDirection.None, 0); }
            catch (ArgumentException) { exCnter++; }
            catch { }

            try { target.Add(new DockPaneLayoutEngine(new DockPaneBase()), DockDirection.Left, -1); }
            catch (ArgumentException) { exCnter++; }
            catch { }
            Assert.AreEqual(4, exCnter, "無効な引数に対して適切な対処が取れていません。");
        }

        /// <summary>
        ///AddEngine のテスト
        ///</summary>
        [TestMethod()]
        public void AddEngineTest()
        {
            var parent = new DockPaneLayoutEngine(new DockPaneBase());
            var target = new DockPaneLayoutEngine(new DockPaneBase());
            var layout = new DockPaneLayoutEngine(new DockPaneBase());
            parent.AddEngine(target);
            target.Parent = parent;
            target.AddEngine(layout);
            layout.Parent = target;

            var parent_acc = DockPaneLayoutEngine_Accessor.AttachShadow(parent);
            Assert.IsTrue(parent_acc.OwnNodes.Contains(parent));
            Assert.IsTrue(parent_acc.OwnNodes.Contains(target));
            Assert.IsTrue(parent_acc.OwnNodes.Contains(layout));
            Assert.AreEqual(3, parent_acc.OwnNodes.Count);

            var target_acc = DockPaneLayoutEngine_Accessor.AttachShadow(target);
            Assert.IsTrue(target_acc.OwnNodes.Contains(target));
            Assert.IsTrue(target_acc.OwnNodes.Contains(layout));
            Assert.AreEqual(2, target_acc.OwnNodes.Count);
        }

        /// <summary>
        ///GetChildrenOf のテスト
        ///</summary>
        [TestMethod()]
        public void GetChildrenOfTest()
        {
            var target = new DockPaneLayoutEngine(new DockPaneBase());
            var lv1A = new DockPaneLayoutEngine(new DockPaneBase());
            var lv1B = new DockPaneLayoutEngine(new DockPaneBase());
            var lv1C = new DockPaneLayoutEngine(new DockPaneBase());
            var lv2A = new DockPaneLayoutEngine(new DockPaneBase());

            target.Add(lv1A, DockDirection.Top);
            target.Add(lv1B, DockDirection.Left);
            target.Add(lv1C, DockDirection.Top);
            lv1C.Add(lv2A, DockDirection.Right);

            //自ノードを含まない
            var testA = target.GetChildrenOf(DockDirection.Left, deep: false).ToArray();
            Assert.IsTrue(testA.Contains(lv1A));
            Assert.IsTrue(testA.Contains(lv1B));
            Assert.AreEqual(2, testA.Length);

            //自ノードを含む
            var testB = target.GetChildrenOf(DockDirection.Right, deep: false).ToArray();
            Assert.IsTrue(testB.Contains(lv1A));
            Assert.IsTrue(testB.Contains(lv1C));
            Assert.IsTrue(testB.Contains(target));
            Assert.AreEqual(3, testB.Length);

            //自ノードを含み、かつ実質接触要素
            var testC = target.GetChildrenOf(DockDirection.Right, deep: true).ToArray();
            Assert.IsTrue(testC.Contains(target));
            Assert.IsTrue(testC.Contains(lv1A));
            Assert.IsTrue(testC.Contains(lv2A));
            Assert.AreEqual(3, testC.Length);

            //自ノードを含み、スキップ2使用(lv1Cから探索)
            var testD = target.GetChildrenOf(DockDirection.Right, 2).ToArray();
            Assert.IsTrue(testD.Contains(target));
            Assert.IsTrue(testD.Contains(lv2A));
            Assert.AreEqual(2, testD.Length);
        }

        /// <summary>
        ///GetSucceedingDireWhenDeletingPane のテスト
        ///</summary>
        [TestMethod()]
        public void GetSucceedingDireWhenDeletingPaneTest()
        {
            var nA = new DockPaneLayoutEngine(new DockPaneBase());
            var nB = new DockPaneLayoutEngine(new DockPaneBase());
            var nC = new DockPaneLayoutEngine(new DockPaneBase());
            var parent = new DockBayLayoutEngine(new DockBayBase());

            var nB_acc = DockPaneLayoutEngine_Accessor.AttachShadow(nB);
            var parent_acc = DockBayLayoutEngine_Accessor.AttachShadow(parent);

            //parent
            //-nA
            //-nB
            // -nC
            parent.Add(nA, DockDirection.Top);
            parent.Add(nB, DockDirection.Left);
            nB.Add(nC, DockDirection.Top);

            Assert.IsTrue(nA.GetSucceedingDireWhenDeletingPane() == DockDirection.Left,
                "DockBayNeigh下での挙動に異常があります。");
            Assert.IsTrue(nB.GetSucceedingDireWhenDeletingPane() == DockDirection.Top,
                "DockPaneNeigh下での挙動に異常があります。");
            Assert.IsTrue(nC.GetSucceedingDireWhenDeletingPane() == DockDirection.Bottom,
                "DockPaneNeigh下での挙動に異常があります。");
        }

        /// <summary>
        ///Initialize のテスト
        ///</summary>
        [TestMethod()]
        public void InitializeTest()
        {
            var target = new DockPaneLayoutEngine(new DockPaneBase());
            var parent = new DockPaneLayoutEngine(new DockPaneBase());
            var owner = new DockBayLayoutEngine(new DockBayBase());
            var align = DockDirection.Top;

            target.Initialize(parent, owner, align);
            Assert.AreEqual(target.Parent, parent);
            Assert.AreEqual(target.Owner, owner);
        }

        /// <summary>
        ///MoveSplitter のテスト
        ///</summary>
        [TestMethod()]
        public void MoveSplitterTest()
        {
            //DockPaneBase target1 = null; // TODO: 適切な値に初期化してください
            //DockPaneNeigh neigh = null; // TODO: 適切な値に初期化してください
            //DockPaneLayoutEngine target = new DockPaneLayoutEngine(target1, neigh); // TODO: 適切な値に初期化してください
            //DockDirection align = new DockDirection(); // TODO: 適切な値に初期化してください
            //double distance = 0F; // TODO: 適切な値に初期化してください
            //target.MoveSplitter(align, distance);
            //Assert.Inconclusive("値を返さないメソッドは確認できません。");
        }

        /// <summary>
        ///OnRemoved のテスト
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DockingWindow.dll")]
        public void OnRemovedTest()
        {
            var target = new DockPaneLayoutEngine_Accessor(new DockPaneBase());
            var flg_Removed = false;
            target.add_Removed((sender, e) => flg_Removed = true);
            target.OnRemoved(new EventArgs());

            Assert.IsTrue(flg_Removed);
        }

        /// <summary>
        ///Remove のテスト
        ///</summary>
        [TestMethod()]
        public void RemoveTest()
        {
            //テスト事項
            //Bay: 子要素の最初の時
            //Pane: 子要素がある場合の処理, 独身の場合

            var bay = new DockBayLayoutEngine(new DockBayBase());
            var lv1A = new DockPaneLayoutEngine(new DockPaneBase());
            var lv1B = new DockPaneLayoutEngine(new DockPaneBase());
            var lv2A = new DockPaneLayoutEngine(new DockPaneBase());
            var flg_removed = false;
            bay.PaneRemoved += (sender, e) => flg_removed = true;

            //lv0A
            //-lv1A(Top)
            //-lv1B(Left)
            // -lv2A(Bottom)
            bay.Add(lv1A, DockDirection.Top);
            bay.Add(lv1B, DockDirection.Left);
            lv1B.Add(lv2A, DockDirection.Bottom);

            var lv1ASeq = new[]
                {
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = lv2A, Right = (DockPaneLayoutEngine)null, Align = DockDirection.Top, Parent = (DockNodeLayoutEngine)bay },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = lv2A, Right = (DockPaneLayoutEngine)null, Align = DockDirection.Top, Parent = (DockNodeLayoutEngine)bay },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = lv2A, Right = (DockPaneLayoutEngine)null, Align = DockDirection.Top, Parent = (DockNodeLayoutEngine)bay },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = (DockPaneLayoutEngine)null, Align = DockDirection.None, Parent = (DockNodeLayoutEngine)null },
                };
            var lv1BSeq = new[]
                {
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = (DockPaneLayoutEngine)null, Align = DockDirection.None, Parent = (DockNodeLayoutEngine)null },
                    new { Top = lv1B, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = lv2A, Align = DockDirection.Bottom, Parent = (DockNodeLayoutEngine)lv2A },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = (DockPaneLayoutEngine)null, Align = DockDirection.None, Parent = (DockNodeLayoutEngine)null },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = (DockPaneLayoutEngine)null, Align = DockDirection.None, Parent = (DockNodeLayoutEngine)null },
                };
            var lv2ASeq = new[]
                {
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = lv2A, Align = DockDirection.Left, Parent = (DockNodeLayoutEngine)bay },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = lv1B, Left = (DockPaneLayoutEngine)null, Right = lv2A, Align = DockDirection.Left, Parent = (DockNodeLayoutEngine)bay },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = lv2A, Align = DockDirection.Left, Parent = (DockNodeLayoutEngine)bay },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = (DockPaneLayoutEngine)null, Align = DockDirection.Left, Parent = (DockNodeLayoutEngine)bay },
                };
            var testFunc = (Action<int>)(idx =>
                {
                    Assert.AreEqual(lv1ASeq[idx].Align, lv1A.Align);
                    Assert.AreEqual(lv1ASeq[idx].Parent, lv1A.Parent);
                    Assert.AreEqual(lv1ASeq[idx].Top, lv1A.Top);
                    Assert.AreEqual(lv1ASeq[idx].Bottom, lv1A.Bottom);
                    Assert.AreEqual(lv1ASeq[idx].Left, lv1A.Left);
                    Assert.AreEqual(lv1ASeq[idx].Right, lv1A.Right);

                    Assert.AreEqual(lv1BSeq[idx].Align, lv1B.Align);
                    Assert.AreEqual(lv1BSeq[idx].Parent, lv1B.Parent);
                    Assert.AreEqual(lv1BSeq[idx].Top, lv1B.Top);
                    Assert.AreEqual(lv1BSeq[idx].Bottom, lv1B.Bottom);
                    Assert.AreEqual(lv1BSeq[idx].Left, lv1B.Left);
                    Assert.AreEqual(lv1BSeq[idx].Right, lv1B.Right);

                    Assert.AreEqual(lv2ASeq[idx].Align, lv2A.Align);
                    Assert.AreEqual(lv2ASeq[idx].Parent, lv2A.Parent);
                    Assert.AreEqual(lv2ASeq[idx].Top, lv2A.Top);
                    Assert.AreEqual(lv2ASeq[idx].Bottom, lv2A.Bottom);
                    Assert.AreEqual(lv2ASeq[idx].Left, lv2A.Left);
                    Assert.AreEqual(lv2ASeq[idx].Right, lv2A.Right);
                });

            //lv0A
            //-lv1A(Top)
            //-lv2A(Bottom->Left)
            bay.Remove(lv1B);
            testFunc(0);

            //lv0A
            //-lv1A(Top)
            //-lv2A(Bottom->Left)
            // -lv1B(Left->Bottom)
            lv2A.Add(lv1B, DockDirection.Bottom);
            testFunc(1);

            //lv0A
            //-lv1A(Top)
            //-lv2A(Bottom->Left)
            lv2A.Remove(lv1B);
            testFunc(2);

            //lv0A
            //-lv2A(Bottom->Left)
            bay.Remove(lv1A);
            testFunc(3);

            Assert.IsTrue(flg_removed, "Removed eventが発生してません。");
        }

        /// <summary>
        ///RemoveEngine のテスト
        ///</summary>
        [TestMethod()]
        public void RemoveEngineTest()
        {
            var parent = new DockPaneLayoutEngine(new DockPaneBase());
            var target = new DockPaneLayoutEngine(new DockPaneBase());
            var layout = new DockPaneLayoutEngine(new DockPaneBase());
            parent.AddEngine(target);
            target.Parent = parent;
            target.AddEngine(layout);
            layout.Parent = target;
            target.RemoveEngine(layout);

            var parent_acc = DockPaneLayoutEngine_Accessor.AttachShadow(parent);
            Assert.IsTrue(parent_acc.OwnNodes.Contains(parent));
            Assert.IsTrue(parent_acc.OwnNodes.Contains(target));
            Assert.IsFalse(parent_acc.OwnNodes.Contains(layout));
            Assert.AreEqual(2, parent_acc.OwnNodes.Count);

            var target_acc = DockPaneLayoutEngine_Accessor.AttachShadow(target);
            Assert.IsTrue(target_acc.OwnNodes.Contains(target));
            Assert.IsFalse(target_acc.OwnNodes.Contains(layout));
            Assert.AreEqual(1, target_acc.OwnNodes.Count);
        }

        /// <summary>
        ///ResizableDistance のテスト
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DockingWindow.dll")]
        public void ResizableDistanceTest()
        {
            //PrivateObject param0 = null; // TODO: 適切な値に初期化してください
            //DockPaneLayoutEngine_Accessor target = new DockPaneLayoutEngine_Accessor(param0); // TODO: 適切な値に初期化してください
            //DockDirection align = new DockDirection(); // TODO: 適切な値に初期化してください
            //double expected = 0F; // TODO: 適切な値に初期化してください
            //double actual;
            //actual = target.ResizableDistance(align);
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("このテストメソッドの正確性を確認します。");
        }

        /// <summary>
        ///ResizeNodes のテスト
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DockingWindow.dll")]
        public void ResizeNodesTest()
        {
            //var args = new[]
            //    {
            //        new{Neigh = new DockPaneNeigh(), Align = DockDirection.Top},
            //        new{Neigh = new DockPaneNeigh(), Align = DockDirection.Left},
            //        new{Neigh = new DockPaneNeigh(), Align = DockDirection.Bottom},
            //        new{Neigh = new DockPaneNeigh(), Align = DockDirection.Left},
            //    }
            //    .Select(a => new {
            //        Neigh = a.Neigh, Align = a.Align,
            //        LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase(), a.Neigh)})
            //    .ToArray();

            //DockBaseNeigh nNode = new DockBayNeigh();
            //DockBaseLayoutEngine lNode = new DockBayLayoutEngine(new DockBayBase(), (DockBayNeigh)nNode);
            //foreach (var aaa in args)
            //{
            //    lNode.Add(aaa.LayoutEngine, aaa.Align, 0.25);
            //    nNode.Add(aaa.Neigh, aaa.Align);

            //    nNode = aaa.Neigh;
            //    lNode = aaa.LayoutEngine;
            //}

            //var target = DockPaneLayoutEngine_Accessor.AttachShadow(args[0].LayoutEngine);
            //target.ResizeNodes(DockDirection.Left, 30);
            //Assert.Inconclusive("値を返さないメソッドは確認できません。");
        }
    }
}
