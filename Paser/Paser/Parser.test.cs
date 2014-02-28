using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace ExpressionSample
{
    [TestFixture]
    public partial class Node
    {
        //[Test]
        //public void hoge()
        //{
        //    var node = new Node("2+3");

        //    node.Parse();
        //    Assert.True(node.Left.Expression  == "2");
        //    Assert.True(node.Right.Expression == "3");
        //    Assert.True(node.Expression       == "+");
        //}


        #region 演算子検索のテスト
        [Test]
        [TestCase("2+3", Result=1)]
        [TestCase("20+3", Result=2)]
        [TestCase("200+3", Result=3)]
        [TestCase("2-3", Result=1)]
        [TestCase("20-3", Result=2)]
        [TestCase("200-3", Result=3)]
        [TestCase("abc-x", Result=3)]
        public int pos_２項_加算減算(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("2*3", Result=1)]
        [TestCase("20*3", Result=2)]
        [TestCase("200*3", Result=3)]
        [TestCase("2/3", Result=1)]
        [TestCase("20/3", Result=2)]
        [TestCase("200/3", Result=3)]
        public int pos_２項_乗算除算(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("2+3*4", Result=1)]
        [TestCase("2*3*4", Result=1)]
        [TestCase("2*3+4", Result=3)]
        public int pos_３項_乗算除算_加算減算(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("2+3,4", Result=3)]
        public int pos_３項_カンマ(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("(3+2)+4", Result=5)]
        [TestCase("(3+2)", Result=-1)]
        [TestCase("((3+2))", Result=-1)]
        [TestCase("((3+2))+4", Result=7)]
        [TestCase("((3+2)+1)+4", Result=9)]
        [TestCase("Sum(3+2)+4", Result=8)]
        [TestCase("2+Sum(3+2)+4", Result=1)]
        public int pos_かっこ(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }

        [Test]
        [TestCase("((3+2)+1)+4)", Result=9)]
        [TestCase("(((3+2)+1)+4", Result=9)]
        [TestCase("2+Sum((3+2)+4", Result=1)]
        [TestCase("2+Sum(3+2))+4", Result=1)]
        [ExpectedException(typeof(Exception))]
        public int pos_かっこ_例外(string expr)
        {
            return Node.GetOperatorPosition(expr);
        }        
        #endregion

        [Test]
        public void parse_２項()
        {
            var node = new Node("2+3");

            node.Parse();
            Assert.True(node.Left.Expression  == "2");
            Assert.True(node.Right.Expression == "3");
            Assert.True(node.Expression       == "+");
            return;
        }

        [Test]
        public void parse_３項()
        {
            var node = new Node("2+3+4");

            node.Parse();
            Assert.True(node.Left.Expression  == "2");
            Assert.True(node.Expression       == "+");

            Assert.True(node.Right.Left.Expression  == "3");
            Assert.True(node.Right.Expression       == "+");
            Assert.True(node.Right.Right.Expression == "4");
            return;
        }

        [Test]
        public void parse_３項カッコあり()
        {
            var node = new Node("(2+3+4)");

            node.Parse();
            Assert.True(node.Left.Expression  == "2");
            Assert.True(node.Expression       == "+");

            Assert.True(node.Right.Left.Expression  == "3");
            Assert.True(node.Right.Expression       == "+");
            Assert.True(node.Right.Right.Expression == "4");
            return;
        }
        [Test]
        public void parse_4項カッコあり()
        {
            var node = new Node("(2+3+4)+5");

            node.Parse();
            {
                Assert.True(node.Left.Left.Expression  == "2");
                Assert.True(node.Left.Expression       == "+");
                {
                    Assert.True(node.Left.Right.Left.Expression  == "3");
                    Assert.True(node.Left.Right.Expression       == "+");
                    Assert.True(node.Left.Right.Right.Expression == "4");
                }

            }
            Assert.True(node.Expression             == "+");
            Assert.True(node.Right.Expression       == "5");
            return;
        }

        [Test]
        public void parse_関数()
        {
            var node = new Node("SUM(2)");

            node.Parse();
            {
                Assert.True(node.Left.Expression       == "2");

            }
            Assert.True(node.Expression             == "SUM");
            Assert.True(node.Right                  == null);

            return;
        }

        [Test]
        public void parse_関数2()
        {
            var node = new Node("SUM(2+3)");

            node.Parse();
            {
                Assert.True(node.Left.Left.Expression       == "2");
                Assert.True(node.Left.Expression            == "+");
                Assert.True(node.Left.Right.Expression      == "3");

            }
            Assert.True(node.Expression             == "SUM");
            Assert.True(node.Right                  == null);

            return;
        }

    }
}

