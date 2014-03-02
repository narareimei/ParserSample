﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
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

        #region 式解析のテスト
        [Test]
        public void parse_１項_1()
        {
            var node = new Node("2");

            node.Parse();
            Assert.True(node.Expression == "2");
            Assert.True(node.Type       == NodeType.Constant);
            return;
        }
        [Test]
        public void parse_１項_2()
        {
            var node = new Node("+2");

            node.Parse();
            Assert.True(node.Expression         == "+");
            Assert.True(node.Type               == NodeType.Operator);
            Assert.True(node.Right.Expression   == "2");
            return;
        }
        [Test]
        public void parse_１項_3()
        {
            var node = new Node("-2");

            node.Parse();
            Assert.True(node.Expression == "-");
            Assert.True(node.Right.Expression == "2");
            return;
        }
        [Test]
        public void parse_２項()
        {
            var node = new Node("2+3");

            node.Parse();
            Assert.True(node.Left.Expression    == "2");
            Assert.True(node.Expression         == "+");
            Assert.True(node.Type               == NodeType.Operator);
            Assert.True(node.Right.Expression   == "3");
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
        public void parse_2項変数()
        {
            var node = new Node("2+abc");

            node.Parse();

            Assert.True(node.Left.Expression    == "2");
            Assert.True(node.Left.Type == NodeType.Constant);

            Assert.True(node.Expression         == "+");
            Assert.True(node.Type               == NodeType.Operator);

            Assert.True(node.Right.Expression   == "abc");
            Assert.True(node.Right.Type         == NodeType.Item);
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
                Assert.True(node.Left.Left.Expression  == "2");
                Assert.True(node.Left.Expression       == "+");
                Assert.True(node.Left.Right.Expression == "3");

            }
            Assert.True(node.Expression     == "SUM");
            Assert.True(node.Type           == NodeType.Function);
            Assert.True(node.Right          == null);
            return;
        }
        [Test]
        public void parse_関数3()
        {
            var node = new Node("SUM(2,3)");

            node.Parse();
            {
                Assert.True(node.Left.Left.Expression  == "2");
                Assert.True(node.Left.Expression       == ",");
                Assert.True(node.Left.Right.Expression == "3");

            }
            Assert.True(node.Expression     == "SUM");
            Assert.True(node.Type           == NodeType.Function);
            Assert.True(node.Right          == null);
            return;
        }
        #endregion

        [Test]
        public void ツリーを追いかけてみる()
        {
            var node = new Node("2+(3+4)");

            node.Parse();

            //Console.WriteLine(node.Left.Expression + " ");
            //Console.WriteLine(node.Expression + " ");
            //Console.WriteLine(node.Right.Expression + " ");

            PutElement(node);

            return;
        }

        public void PutElement(Node node)
        {
            if(node == null)
            {
                return ;
            }
            if (node.Left != null)
            {
                PutElement(node.Left);
            }
            if (node.Right != null)
            {
                PutElement(node.Right);
            }
            if (node.Expression != null)
            {
                Console.WriteLine(node.Expression + " ");
            }
        }

        public int GetElement(Node node)
        {
            if (node == null)
            {
                throw new Exception("ノードがNullです"); ;
            }

            // 1.左 演算子 右
            // 2.子要素なし
            // 3.

            var leftValue = 0;
            var rightValue = 0;

            if (node.Left != null)
            {
                leftValue =  GetElement(node.Left);
            }
            if (node.Right != null)
            {
                rightValue = GetElement(node.Right);
            }
            if (node.Expression != null)
            {
                Console.WriteLine(node.Expression + " ");
            }
            return 0;
        }


        [Test]
        [TestCase("1", Result = 1)]
        [TestCase("2", Result = 2)]
        [TestCase("-2", Result = -2)]
        [TestCase("1-2", Result = -1)]
        [TestCase("2*3", Result = 6)]
        [TestCase("6/3", Result = 2)]
        [TestCase("6/3+1", Result = 3)]
        [TestCase("2*(2+3)", Result = 10)]
        [TestCase("2*(2+3)-(5-3)*2", Result = 6)]
        public int 計算(string expression)
        {
            var node = new Node(expression);

            node.Parse();
            var ans = node.Compute();

            return ans;
        }
    }
}

