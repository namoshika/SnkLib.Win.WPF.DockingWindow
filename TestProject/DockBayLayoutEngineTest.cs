using SunokoLibrary.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;

namespace TestProject
{
    /// <summary>
    ///DockBayLayoutEngineTest のテスト クラスです。すべての
    ///DockBayLayoutEngineTest 単体テストをここに含めます
    ///</summary>
    [TestClass()]
    public class DockBayLayoutEngineTest
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
        ///Add のテスト
        ///</summary>
        [TestMethod()]
        public void AddTest()
        {
            var eTarget = new DockBayLayoutEngine(new DockBayBase());

            //AddTest
            //DockPaneNeighs
            var tmp = new[]
                {
                    new {LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Top},
                    new {LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Left},
                    new {LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Top},
                    new {LayoutEngine = new DockPaneLayoutEngine(new DockPaneBase()), Align = DockDirection.Right}
                }
                .ToArray();
            //TestDatas
            var testDtIdx = 0;
            var testDt = new[]
                {
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = (DockPaneLayoutEngine)null },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = (DockPaneLayoutEngine)null, Right = tmp[1].LayoutEngine },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = tmp[2].LayoutEngine, Left = (DockPaneLayoutEngine)null, Right = (DockPaneLayoutEngine)null },
                    new { Top = (DockPaneLayoutEngine)null, Bottom = (DockPaneLayoutEngine)null, Left = tmp[3].LayoutEngine, Right = (DockPaneLayoutEngine)null },
                };
            //MargeCollection
            var children = tmp
                .Select(n => new
                {
                    Align = n.Align,
                    LayoutEngine = n.LayoutEngine,
                    TestData = testDt[testDtIdx++],
                });
            foreach (var item in children)
            {
                eTarget.Add(item.LayoutEngine, item.Align);

                Assert.AreEqual(eTarget, item.LayoutEngine.Parent);
                Assert.AreEqual(item.TestData.Top, item.LayoutEngine.Top);
                Assert.AreEqual(item.TestData.Bottom, item.LayoutEngine.Bottom);
                Assert.AreEqual(item.TestData.Left, item.LayoutEngine.Left);
                Assert.AreEqual(item.TestData.Right, item.LayoutEngine.Right);
            }

            //InsertTest
            var lyout = new DockPaneLayoutEngine(new DockPaneBase());
            eTarget.Add(lyout, DockDirection.Left, 1);
            Assert.AreEqual(tmp[2].LayoutEngine, lyout.Top);
            Assert.AreEqual(tmp[1].LayoutEngine, lyout.Left);
            Assert.AreEqual(lyout, lyout.Right);
            Assert.AreEqual(null, lyout.Bottom);
            Assert.AreEqual(tmp[0].LayoutEngine.Left, lyout);
            Assert.AreEqual(eTarget, lyout.Parent);
        }

        /// <summary>
        ///GetChildrenOf のテスト
        ///</summary>
        [TestMethod()]
        public void GetChildrenOfTest()
        {
            var target = new DockBayLayoutEngine(new DockBayBase());
            var lv1A = new DockPaneLayoutEngine(new DockPaneBase());
            var lv1B = new DockPaneLayoutEngine(new DockPaneBase());
            var lv1C = new DockPaneLayoutEngine(new DockPaneBase());
            var lv1D = new DockPaneLayoutEngine(new DockPaneBase());
            var lv2A = new DockPaneLayoutEngine(new DockPaneBase());

            target.Add(lv1A, DockDirection.Top);

            target.Add(lv1D, DockDirection.Top);
            lv1D.Add(lv2A, DockDirection.Right);
            target.Add(lv1C, DockDirection.Left);
            target.Add(lv1B, DockDirection.Top);

            //自ノードを含まない
            var testA = target.GetChildrenOf(DockDirection.Left, deep: false).ToArray();
            Assert.IsTrue(testA.Contains(lv1B));
            Assert.IsTrue(testA.Contains(lv1C));
            Assert.AreEqual(2, testA.Length);

            //自ノードを含む
            var testB = target.GetChildrenOf(DockDirection.Right, deep: false).ToArray();
            Assert.IsTrue(testB.Contains(lv1B));
            Assert.IsTrue(testB.Contains(lv1D));
            Assert.IsTrue(testB.Contains(lv1A));
            Assert.AreEqual(3, testB.Length);

            //自ノードを含み、かつ実質接触要素
            var testC = target.GetChildrenOf(DockDirection.Right, deep: true).ToArray();
            Assert.IsTrue(testC.Contains(lv1A));
            Assert.IsTrue(testC.Contains(lv1B));
            Assert.IsTrue(testC.Contains(lv2A));
            Assert.AreEqual(3, testC.Length);

            //自ノードを含み、スキップ2使用(lv1Cから探索)
            var testD = target.GetChildrenOf(DockDirection.Right, 2).ToArray();
            Assert.IsTrue(testD.Contains(lv1A));
            Assert.IsTrue(testD.Contains(lv2A));
            Assert.AreEqual(2, testD.Length);
        }

        /// <summary>
        ///OnDockPaneAdded のテスト
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DockingWindow.dll")]
        public void OnDockPaneAddedTest()
        {
            var target = new DockBayLayoutEngine(new DockBayBase());
            var neighA = new DockPaneLayoutEngine(new DockPaneBase());
            var neighB = new DockPaneLayoutEngine(new DockPaneBase());
            var idx_logger = 0;
            var logger = new DockDirection[2];
            target.PaneAddedInBay += (sender, e) =>
            {
                logger[idx_logger++] = e.Align;
                if (idx_logger > 2)
                    Assert.Fail("DockPaneAddedの呼び出し回数が多すぎです");
            };
            target.Add(neighA, DockDirection.Top);
            neighA.Add(neighB, DockDirection.Left);

            Assert.AreEqual<int>(idx_logger, 2, "DockPaneAddedの呼び出し回数が少なすぎです。");
        }

        /// <summary>
        ///OnDockPaneRemoved のテスト
        ///</summary>
        [TestMethod()]
        [DeploymentItem("DockingWindow.dll")]
        public void OnDockPaneRemovedTest()
        {
            var target = new DockBayLayoutEngine(new DockBayBase());
            var neighA = new DockPaneLayoutEngine(new DockPaneBase());
            var neighB = new DockPaneLayoutEngine(new DockPaneBase());
            var logger = new bool[2];
            var counter = 0;
            target.PaneRemovedInBay += (sender, e) =>
            {
                //[0]はneighAのeventが呼ばれたかを記録
                //[1]はneighBのeventが呼ばれたかを記録
                logger[0] |= neighA == e.DockPane;
                logger[1] |= neighB == e.DockPane;
                counter++;
                if (counter > 2)
                    Assert.Fail("DockPaneAddedの呼び出し回数が多すぎです");
            };
            target.Add(neighA, DockDirection.Top);
            neighA.Add(neighB, DockDirection.Left);
            neighA.Remove(neighB);
            target.Remove(neighA);

            Assert.AreEqual<int>(counter, 2, "DockPaneAddedの呼び出し回数が少なすぎです。");
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
            var eBay = new DockBayLayoutEngine(new DockBayBase());
            var eLv1A = new DockPaneLayoutEngine(new DockPaneBase());
            var eLv1B = new DockPaneLayoutEngine(new DockPaneBase());
            var eLv1C = new DockPaneLayoutEngine(new DockPaneBase());

            //lv0A
            //-lv1A(Top)
            //-lv1B(Left)
            //-lv1C(Right)
            eBay.Add(eLv1A, DockDirection.Top);
            eBay.Add(eLv1B, DockDirection.Left);
            eBay.Add(eLv1C, DockDirection.Right);

            //bay
            //-lv1A(Top)
            //-lv1C(Right)
            eBay.Remove(eLv1B);
            //LeftPane
            Assert.IsNull(eLv1A.Top);
            Assert.IsNull(eLv1A.Bottom);
            Assert.IsNull(eLv1A.Left);
            Assert.AreNotEqual(eLv1B, eLv1A.Right);
            Assert.AreEqual(eLv1C, eLv1A.Right);
            //RightPane
            Assert.IsNull(eLv1C.Top);
            Assert.IsNull(eLv1C.Bottom);
            Assert.AreNotEqual(eLv1B, eLv1C.Left);
            Assert.AreEqual(eLv1C, eLv1C.Left);
            Assert.IsNull(eLv1C.Right);

            //lv0A
            //-lv2A(Bottom->Left)
            eBay.Remove(eLv1A);
            Assert.IsNull(eLv1C.Left);
        }
    }
}