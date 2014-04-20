using SunokoLibrary.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject
{
    /// <summary>
    ///DockBayNeighTest のテスト クラスです。すべての
    ///DockBayNeighTest 単体テストをここに含めます
    ///</summary>
    [TestClass()]
    public class DockBayNeighTest
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
            var target = new DockBayLayoutEngine(new DockBayBase());
            var flg_PaneAdded = false;

            target.PaneAdded += (sender, e) => flg_PaneAdded = true;
            var testDt = new[]
                {
                    new { Neigh = new DockPaneLayoutEngine(new DockPaneBase()), Index = 0, Count = 1, Align = DockDirection.Left },
                    new { Neigh = new DockPaneLayoutEngine(new DockPaneBase()), Index = 1, Count = 2, Align = DockDirection.Top },
                    new { Neigh = new DockPaneLayoutEngine(new DockPaneBase()), Index = 1, Count = 3, Align = DockDirection.Top },
                };
            foreach (var item in testDt)
            {
                target.Add(item.Neigh, item.Align, item.Index);
                Assert.AreEqual(item.Align, item.Neigh.Align);
                Assert.AreEqual(target, item.Neigh.Parent);
                Assert.AreEqual(target, item.Neigh.Owner);
                Assert.AreEqual(item.Index, target.Children.IndexOf(item.Neigh));
                Assert.AreEqual(item.Count, target.Children.Count);
            }
            Assert.IsTrue(flg_PaneAdded, "子要素追加時にPaneAddedイベントが発動されていません。");

            //不正引数用
            var exCnter = 0;
            try { target.Add((DockPaneLayoutEngine)null, DockDirection.Top, 0); }
            catch (ArgumentNullException) { exCnter++; }
            catch { }

            try { target.Add(testDt[0].Neigh, DockDirection.Left, 0); }
            catch (ArgumentException) { exCnter++; }
            catch { }

            try { target.Add(new DockPaneLayoutEngine(new DockPaneBase()), DockDirection.None, 0); }
            catch (ArgumentException) { exCnter++; }
            catch { }

            try { target.Add(new DockPaneLayoutEngine(new DockPaneBase()), DockDirection.Bottom, -1); }
            catch (ArgumentException) { exCnter++; }
            catch { }
            Assert.AreEqual(4, exCnter, "無効な引数に対して適切な対処が取れていません。");
        }
    }
}
