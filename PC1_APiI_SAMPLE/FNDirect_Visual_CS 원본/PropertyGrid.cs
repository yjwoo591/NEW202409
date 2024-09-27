using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Reflection;  
using System.Xml;
using System.Xml.Serialization;

 
namespace FNDirectTest
{    
    [Serializable()]
    public class PropertyGrid_0
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _Code = "101G3";
        private string _OrderCnt = "1";
        private string _OrderPrice = "241.80";
        private string _Gubun = "1";
        private string _PriceGubun = "1";
        private string _ChegulGubun = "1";
        private string _Acctno2 = "00156028";
        private string _Passwd2 = "1111";
        private string _Code2 = "101G3";
        private string _OrderCnt2 = "1";
        private string _OrderPrice2 = "241.80";
        private string _Gubun2 = "1";
        private string _PriceGubun2 = "1";
        private string _ChegulGubun2 = "1";

        private string sInputData;        
        public string GetInputData()
        {            
            sInputData  = _Acctno + "," + _Passwd + "," + _Code + "," + _OrderCnt + "," + _OrderPrice + "," + _Gubun + "," + _PriceGubun + "," + _ChegulGubun + "|" 
                        + _Acctno2 + "," + _Passwd2 + "," + _Code2 + "," + _OrderCnt2 + "," + _OrderPrice2 + "," + _Gubun2 + "," + _PriceGubun2 + "," + _ChegulGubun2;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 종목코드 { get { return _Code; } set { _Code = value; } }
        [Category("Input1"), Description("")] public string 주문수량 { get { return _OrderCnt; } set { _OrderCnt = value; } }
        [Category("Input1"), Description("")] public string 주문가격 { get { return _OrderPrice; } set { _OrderPrice = value; } }
        [Category("Input1"), Description("")] public string 매매구분 { get { return _Gubun; } set { _Gubun = value; }}
        [Category("Input1"), Description("")] public string 가격구분 { get { return _PriceGubun; } set { _PriceGubun = value; } }
        [Category("Input1"), Description("")] public string 체결구분 { get { return _ChegulGubun; } set { _ChegulGubun = value; } }

        [Category("Input2"), Description("")] public string 계좌번호2 { get{ return _Acctno2; } set{ _Acctno2 = value; }}
        [Category("Input2"), Description("")] public string 비밀번호2 { get { return _Passwd2; } set { _Passwd2 = value; }}
        [Category("Input2"), Description("")] public string 종목코드2 { get { return _Code2; } set { _Code2 = value; } }
        [Category("Input2"), Description("")] public string 주문수량2 { get { return _OrderCnt2; } set { _OrderCnt2 = value; } }
        [Category("Input2"), Description("")] public string 주문가격2 { get { return _OrderPrice2; } set { _OrderPrice2 = value; } }
        [Category("Input2"), Description("")] public string 매매구분2 { get { return _Gubun2; } set { _Gubun2 = value; }}
        [Category("Input2"), Description("")] public string 가격구분2 { get { return _PriceGubun2; } set { _PriceGubun2 = value; } }
        [Category("Input2"), Description("")] public string 체결구분2 { get { return _ChegulGubun2; } set { _ChegulGubun2 = value; } }
    }
    
    public class PropertyGrid_1
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _No = "1";
        private string _Cnt = "1";
        private string _Acctno2 = "00156028";
        private string _Passwd2 = "1111";
        private string _No2 = "1";
        private string _Cnt2 = "1";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _No + "," + _Cnt + "|"
                       + _Acctno2 + "," + _Passwd2 + "," + _No2 + "," + _Cnt2;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 주문번호 { get { return _No; } set { _No = value; } }
        [Category("Input1"), Description("")] public string 취소주문수량 { get { return _Cnt; } set { _Cnt = value; } }
   
        [Category("Input2"), Description("")] public string 계좌번호2 { get{ return _Acctno2; } set{ _Acctno2 = value; }}
        [Category("Input2"), Description("")] public string 비밀번호2 { get { return _Passwd2; } set { _Passwd2 = value; }}
        [Category("Input2"), Description("")] public string 주문번호2 { get { return _No2; } set { _No2 = value; } }
        [Category("Input2"), Description("")] public string 취소주문수량2 { get { return _Cnt2; } set { _Cnt2 = value; } }
    }

    public class PropertyGrid_2
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _No = "1";
        private string _Cnt = "1";
        private string _Price = "241.80";
        private string _Acctno2 = "00156028";
        private string _Passwd2 = "1111";
        private string _No2 = "1";
        private string _Cnt2 = "1";
        private string _Price2 = "241.80";
   

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _No + "," + _Cnt + "," + _Price + "|" +
                         _Acctno2 + "," + _Passwd2 + "," + _No2 + "," + _Cnt2 + "," + _Price2;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 주문번호 { get { return _No; } set { _No = value; } }
        [Category("Input1"), Description("")] public string 정정수량 { get { return _Cnt; } set { _Cnt = value; } }
        [Category("Input1"), Description("")] public string 정정가격 { get { return _Price; } set { _Price = value; } }
   
        [Category("Input2"), Description("")] public string 계좌번호2 { get{ return _Acctno2; } set{ _Acctno2 = value; }}
        [Category("Input2"), Description("")] public string 비밀번호2 { get { return _Passwd2; } set { _Passwd2 = value; }}
        [Category("Input2"), Description("")] public string 주문번호2 { get { return _No2; } set { _No2 = value; } }
        [Category("Input2"), Description("")] public string 정정수량2 { get { return _Cnt2; } set { _Cnt2 = value; } }
        [Category("Input2"), Description("")] public string 정정가격2 { get { return _Price2; } set { _Price2 = value; } }
    }

    public class PropertyGrid_3
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 계좌번호 { get { return _Acctno; } set { _Acctno = value; } }
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; } }
    }

    public class PropertyGrid_4
    {
        private string _Date = "20111222";
        private string _Acctno = "00156028";
        private string _Passwd = "1111";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Date + "," + _Acctno + "," + _Passwd;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 거래일자 { get { return _Date; } set { _Date = value; } }
        [Category("Input1"), Description("")] public string 계좌번호 { get { return _Acctno; } set { _Acctno = value; } }
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; } }
    }

    public class PropertyGrid_5
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _Code = "101G3";
        private string _Gubun = "1";
        private string _Price = "241.80";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _Code + "," + _Gubun + "," + _Price;
            return sInputData;
        }
        
        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 종목코드 { get { return _Code; } set { _Code = value; } } 
        [Category("Input1"), Description("")] public string 매매구분 { get { return _Gubun; } set { _Gubun = value; }}
        [Category("Input1"), Description("")] public string 가격 { get { return _Price; } set { _Price = value; } }
    }

    public class PropertyGrid_6
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _Code = "6EM12";
        private string _OrderCnt = "1";
        private string _OrderPrice = "13100";
        private string _Price = " ";
        private string _Gubun = "1";
        private string _Gubun1 = "1";
        private string _Gubun2 = "1";
        private string _PriceGubun = "1";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _Code + "," + _Gubun + "," + _Gubun1 + "," + _Gubun2 + ","
                + _PriceGubun + "," + _OrderCnt + "," + _OrderPrice + "," + _Price + ","; // 조건가격이 공백인 경우 끝에 컴마를 추가해야함.
            return sInputData;
        }
        
        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 종목코드 { get { return _Code; } set { _Code = value; } }
        [Category("Input1"), Description("")] public string 매매구분 { get { return _Gubun; } set { _Gubun = value; }}
        [Category("Input1"), Description("")] public string 주문구분 { get { return _Gubun1; } set { _Gubun1 = value; }}
        [Category("Input1"), Description("")] public string 조건구분 { get { return _Gubun2; } set { _Gubun2 = value; }}
        [Category("Input1"), Description("")] public string 가격구분 { get { return _PriceGubun; } set { _PriceGubun = value; } }
        [Category("Input1"), Description("")] public string 주문수량 { get { return _OrderCnt; } set { _OrderCnt = value; } }
        [Category("Input1"), Description("")] public string 주문가격 { get { return _OrderPrice; } set { _OrderPrice = value; } }
        [Category("Input1"), Description("")] public string 조건가격 { get { return _Price; } set { _Price = value; } }       
     }

    public class PropertyGrid_7
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _Code = "6EM12";
        private string _OrderCnt = "1";
        private string _Price = "13120";
        private string _Price2 = " ";
        private string _PreNo = "201204240001";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _PreNo + "," + _Code + "," + _OrderCnt + "," + _Price + "," + _Price2 + ",";// 조건가격이 공백인 경우 끝에 컴마를 추가해야함.
            return sInputData;
        }


        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 원주문번호 { get { return _PreNo; } set { _PreNo = value; } }
        [Category("Input1"), Description("")] public string 종목코드 { get { return _Code; } set { _Code = value; } }
        [Category("Input1"), Description("")] public string 주문수량 { get { return _OrderCnt; } set { _OrderCnt = value; } }
        [Category("Input1"), Description("")] public string 정정가격 { get { return _Price; } set { _Price = value; } }
        [Category("Input1"), Description("")] public string 정정조건가격 { get { return _Price2; } set { _Price2 = value; } }             
     }

    public class PropertyGrid_50
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _OrgNo = "201204240001";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _OrgNo;
            return sInputData;
        }


        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")]
        public string 계좌번호 { get { return _Acctno; } set { _Acctno = value; } }
        [Category("Input1"), Description("")]
        public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; } }
        [Category("Input1"), Description("")]
        public string 원주문번호 { get { return _OrgNo; } set { _OrgNo = value; } }
    }

    public class PropertyGrid_8
    {
        private string _Date1 = "20111201";
        private string _Date2 = "20111222";
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _Code = "6EM12";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Date1 + "," + _Date2 + "," + _Acctno + "," + _Passwd + "," + _Code;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 시작일자 { get { return _Date1; } set { _Date1 = value; } }
        [Category("Input1"), Description("")] public string 종료일자 { get { return _Date2; } set { _Date2 = value; } }
        [Category("Input1"), Description("")] public string 계좌번호 { get { return _Acctno; } set { _Acctno = value; } }
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; } }
        [Category("Input1"), Description("")] public string 종목코드 { get { return _Code; } set { _Code = value; } }
    }

    public class PropertyGrid_9
    {
        private string _Date1 = "20111201";
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _Code = "USD";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Date1 + "," + _Acctno + "," + _Code + "," + _Passwd;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 조회날짜 { get { return _Date1; } set { _Date1 = value; } }
        [Category("Input1"), Description("")] public string 계좌번호 { get { return _Acctno; } set { _Acctno = value; } }
        [Category("Input1"), Description("")] public string 통화코드 { get { return _Code; } set { _Code = value; } }
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; } }       
    }

    public class PropertyGrid_10
    {
        private string _Code = "CURRCD";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Code;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 코드구분 { get { return _Code; } set { _Code = value; } }       
    }

    public class PropertyGrid_11
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _Code = "6EM12";
        private string _Gubun = "1";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _Code + "," + _Gubun;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 종목코드 { get { return _Code; } set { _Code = value; } } 
        [Category("Input1"), Description("")] public string 매매구분 { get { return _Gubun; } set { _Gubun = value; }}
    }

    public class PropertyGrid_12
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _No = "1";
        private string _Cnt = "1";
        private string _Acctno2 = "00156028";
        private string _Passwd2 = "1111";
        private string _No2 = "2";
        private string _Cnt2 = "1";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _No + "," + _Cnt + "|" +
                        _Acctno2 + "," + _Passwd2 + "," + _No2 + "," + _Cnt2;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 주문번호 { get { return _No; } set { _No = value; } }
        [Category("Input1"), Description("")] public string 잔량 { get { return _Cnt; } set { _Cnt = value; } }
   
        [Category("Input2"), Description("")] public string 계좌번호2 { get{ return _Acctno2; } set{ _Acctno2 = value; }}
        [Category("Input2"), Description("")] public string 비밀번호2 { get { return _Passwd; } set { _Passwd2 = value; }}
        [Category("Input2"), Description("")] public string 주문번호2 { get { return _No2; } set { _No2 = value; } }
        [Category("Input2"), Description("")] public string 잔량2 { get { return _Cnt2; } set { _Cnt2 = value; } }
    }

    public class PropertyGrid_13
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _No = "1";
        private string _Ret = "1";
        private string _Cnt = "1";
        private string _Price = "241.80";
        private string _Imf = "1";
        private string _Acctno2 = "00156028";
        private string _Passwd2 = "1111";
        private string _No2 = "2";
        private string _Ret2 = "1";
        private string _Cnt2 = "1";
        private string _Price2 = "241.80";
        private string _Imf2 = "1";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _No + "," + _Ret + "," + _Cnt + "," + _Price + "," + _Imf + "|" +
                         _Acctno2 + "," + _Passwd2 + "," + _No2 + "," + _Ret2 + "," + _Cnt2 + "," + _Price2 + "," + _Imf2;
            return sInputData;
        } 


        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 계좌번호 { get { return _Acctno; } set { _Acctno = value; } }
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 주문번호 { get { return _No; } set { _No = value; } }
        [Category("Input1"), Description("")] public string 잔량 { get { return _Ret; } set { _Ret = value; } }
        [Category("Input1"), Description("")] public string 정정수량 { get { return _Cnt; } set { _Cnt = value; } }
        [Category("Input1"), Description("")] public string 정정가격 { get { return _Price; } set { _Price = value; } }
        [Category("Input1"), Description("")] public string IFM사용유부 { get { return _Imf; } set { _Imf = value; } }
   
        [Category("Input2"), Description("")] public string 계좌번호2 { get{ return _Acctno2; } set{ _Acctno2 = value; }}
        [Category("Input2"), Description("")] public string 비밀번호2 { get { return _Passwd2; } set { _Passwd2 = value; }}
        [Category("Input2"), Description("")] public string 주문번호2 { get { return _No2; } set { _No2 = value; } }
        [Category("Input2"), Description("")] public string 잔량2 { get { return _Ret2; } set { _Ret2 = value; } }
        [Category("Input2"), Description("")] public string 정정수량2 { get { return _Cnt2; } set { _Cnt2 = value; } }
        [Category("Input2"), Description("")] public string 정정가격2 { get { return _Price2; } set { _Price2 = value; } }
        [Category("Input2"), Description("")] public string IFM사용유부2 { get { return _Imf2; } set { _Imf2 = value; } }
    }

    public class PropertyGrid_14
    {
        private string _Acctno = "00156028";
        private string _Passwd = "1111";
        private string _Code = "101G3";
        private string _Gubun = "1";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Acctno + "," + _Passwd + "," + _Code + "," + _Gubun;
            return sInputData;
        } 

        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 계좌번호 { get{ return _Acctno; } set{ _Acctno = value; }}
        [Category("Input1"), Description("")] public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; }}
        [Category("Input1"), Description("")] public string 종목코드 { get { return _Code; } set { _Code = value; } } 
        [Category("Input1"), Description("")] public string 매매구분 { get { return _Gubun; } set { _Gubun = value; }}
    }

    public class PropertyGrid_15
    {
        private string _Code1 = "101G6";
        private string _Code2 = "101G7";
        private string _Code3 = "101GC";
        private string _Code4 = "101H3";
        private string _Code5 = "165G6";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Code1 + "|" + _Code2 + "|" + _Code3 + "|" + _Code4 + "|" + _Code5;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 종목 { get{ return _Code1; } set{ _Code1 = value; }}
        [Category("Input2"), Description("")] public string 종목2 { get{ return _Code2; } set{ _Code2 = value; }}
        [Category("Input3"), Description("")] public string 종목3 { get{ return _Code3; } set{ _Code3 = value; }}
        [Category("Input4"), Description("")] public string 종목4 { get{ return _Code4; } set{ _Code4 = value; }}
        [Category("Input5"), Description("")] public string 종목5 { get{ return _Code5; } set{ _Code5 = value; }}
    }

    public class PropertyGrid_16
    {
        private string _Code1 = "6AM12";
        private string _Code2 = "6BM12";
        private string _Code3 = "6SM12";
        private string _Code4 = "6JM12";
        private string _Code5 = "6EM12";


        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Code1 + "|" + _Code2 + "|" + _Code3 + "|" + _Code4 + "|" + _Code5;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 종목 { get{ return _Code1; } set{ _Code1 = value; }}
        [Category("Input2"), Description("")] public string 종목2 { get{ return _Code2; } set{ _Code2 = value; }}
        [Category("Input3"), Description("")] public string 종목3 { get{ return _Code3; } set{ _Code3 = value; }}
        [Category("Input4"), Description("")] public string 종목4 { get{ return _Code4; } set{ _Code4 = value; }}
        [Category("Input5"), Description("")] public string 종목5 { get{ return _Code5; } set{ _Code5 = value; }}
    }

    public class PropertyGrid_17
    {
        public string _Type = "1";


        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Type;
            return sInputData;
        }
                
        [Browsable(true)]
        [ReadOnly(false)] 
        [Category("Input1"), Description("")] public string 타입 { get{ return _Type; } set{ _Type = value; }}
    }

    public class PropertyGrid_18
    {
        public string _Value = "101G3";
        public string _Value1 = "1";
        public string _Value2 = "D";
        public string _Value3 = "001";
        public string _Value4 = "99999999";
        public string _Value5 = "N";
        public string _Value6 = "0";
        public string _Value7 = "000";
        public string _Value8 = "000000";
        public string _Value9 = "0250";
        public string _Value10 = "99999999999999999999999";
        public string _Value11 = "N";
        public string _Value12 = "000000000";


        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Value + "," + _Value1 + "," + _Value2 + "," + _Value3 + "," + _Value4 + "," +
                _Value5 + "," + _Value6 + "," + _Value7 + "," + _Value8 + "," + _Value9 + "," +
                _Value10 + "," + _Value11 + "," + _Value12;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")] public string 종목코드 { get { return _Value; } set { _Value = value; } }
        [Category("Input1"), Description("")] public string 구분 { get { return _Value1; } set { _Value1 = value; } }
        [Category("Input1"), Description("")] public string 주기 { get { return _Value2; } set { _Value2 = value; } }
        [Category("Input1"), Description("")] public string 간격 { get { return _Value3; } set { _Value3 = value; } }
        [Category("Input1"), Description("")] public string 기준일자 { get { return _Value4; } set { _Value4 = value; } }
        [Category("Input1"), Description("")] public string 만기보정 { get { return _Value5; } set { _Value5 = value; } }
        [Category("Input1"), Description("")] public string 제외구분 { get { return _Value6; } set { _Value6 = value; } }
        [Category("Input1"), Description("")] public string 제외기준틱 { get { return _Value7; } set { _Value7 = value; } }
        [Category("Input1"), Description("")] public string 제외기준거래량 { get { return _Value8; } set { _Value8 = value; } }
        [Category("Input1"), Description("")] public string 요청건수 { get { return _Value9; } set { _Value9 = value; } }
        [Category("Input1"), Description("")] public string 다음조회키값 { get { return _Value10; } set { _Value10 = value; } }
        [Category("Input1"), Description("")] public string 등락률차트 { get { return _Value11; } set { _Value11 = value; } }
        [Category("Input1"), Description("")] public string 누적갭 { get { return _Value12; } set { _Value12 = value; } }
    }

    public class PropertyGrid_19
    {
        public string _Value = "6EH12";
        public string _Value1 = "1";
        public string _Value2 = "D";
        public string _Value3 = "001";
        public string _Value4 = "99999999";
        public string _Value5 = "N";
        public string _Value6 = "0";
        public string _Value7 = "000";
        public string _Value8 = "000000";
        public string _Value9 = "0250";
        public string _Value10 = "99999999999999999999999";
        public string _Value11 = "N";
        public string _Value12 = "N";
        public string _Value13 = "000000000";


        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Value + "," + _Value1 + "," + _Value2 + "," + _Value3 + "," + _Value4 + "," +
                _Value5 + "," + _Value6 + "," + _Value7 + "," + _Value8 + "," + _Value9 + "," +
                _Value10 + "," + _Value11 + "," + _Value12 + "," + _Value13;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")]
        public string 종목코드 { get { return _Value; } set { _Value = value; } }
        [Category("Input1"), Description("")]
        public string 구분 { get { return _Value1; } set { _Value1 = value; } }
        [Category("Input1"), Description("")]
        public string 주기 { get { return _Value2; } set { _Value2 = value; } }
        [Category("Input1"), Description("")]
        public string 간격 { get { return _Value3; } set { _Value3 = value; } }
        [Category("Input1"), Description("")]
        public string 기준일자 { get { return _Value4; } set { _Value4 = value; } }
        [Category("Input1"), Description("")]
        public string 만기보정 { get { return _Value5; } set { _Value5 = value; } }
        [Category("Input1"), Description("")]
        public string 제외구분 { get { return _Value6; } set { _Value6 = value; } }
        [Category("Input1"), Description("")]
        public string 제외기준틱 { get { return _Value7; } set { _Value7 = value; } }
        [Category("Input1"), Description("")]
        public string 제외기준거래량 { get { return _Value8; } set { _Value8 = value; } }
        [Category("Input1"), Description("")]
        public string 요청건수 { get { return _Value9; } set { _Value9 = value; } }
        [Category("Input1"), Description("")]
        public string 다음조회키값 { get { return _Value10; } set { _Value10 = value; } }
        [Category("Input1"), Description("")]
        public string 본장여부 { get { return _Value11; } set { _Value11 = value; } }
        [Category("Input1"), Description("")]
        public string 등락률차트 { get { return _Value12; } set { _Value12 = value; } }
        [Category("Input1"), Description("")]
        public string 누적갭 { get { return _Value13; } set { _Value13 = value; } }
    }

    public class PropertyGrid_20
    {
        private string _Date1 = "20120901";
        private string _Date2 = "20120906";
        private string _Acctno = "00156028";
        private string _Passwd = "1111";

        private string sInputData;
        public string GetInputData()
        {
            sInputData = _Date1 + "," + _Date2 + "," + _Acctno + "," + _Passwd;
            return sInputData;
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Input1"), Description("")]
        public string 시작일자 { get { return _Date1; } set { _Date1 = value; } }
        [Category("Input1"), Description("")]
        public string 종료일자 { get { return _Date2; } set { _Date2 = value; } }
        [Category("Input1"), Description("")]
        public string 계좌번호 { get { return _Acctno; } set { _Acctno = value; } }
        [Category("Input1"), Description("")]
        public string 비밀번호 { get { return _Passwd; } set { _Passwd = value; } }
    }
}
