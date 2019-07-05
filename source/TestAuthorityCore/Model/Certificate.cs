using Plavy.Core.Common;
using SmartDb.NetCore;
using SmartDb.SQLite.NetCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using TableAttribute = SmartDb.NetCore.TableAttribute;

namespace TestAuthorityCore.Model
{
    [Table(TableName="tb_certs")]
    public class Certificate
    {
        /// <summary>
        /// 证书序列号
        /// </summary>
        ///
        [TableColumn(IsPrimaryKey = true)]
        public string SerialNo { get; set; }

        public string CommonName { get; set; }

        public string CertData { get; set; }

        public string CertPwd { get; set; }

        public DateTime Udt { get; set; }
    }

    public class Store
    {
        public static void Insert(string sSerialNo,string sCertData,string sCommonName,string sPwd)
        {
            string sConn = PlavyConfig.GetConnectionString("db");
            SqlDb db = new SQLiteDb(sConn);
            var dbFactory = db.DbBuilder.DbFactory;
            var dbOperator = dbFactory.GetDbParamOperator();
            db.BeginTransaction();
            var obj = db.Insert<Certificate>(new Certificate() {
                SerialNo = sSerialNo,
                CommonName=sCommonName,
                CertData = sCertData,
                CertPwd = sPwd,
                Udt=DateTime.Now
            });
            db.CommitTransaction();
        }

        public static List<Certificate> Query(string sSerialNo, string sCertData, string sPwd)
        {
            SqlDb db = new SQLiteDb();
            var dbFactory = db.DbBuilder.DbFactory;
            var dbOperator = dbFactory.GetDbParamOperator();

           var dblist = db.Query<Certificate>("SerialNo,CertData,CommonName","1>0");

            return dblist;
        }

        public static object GetMaxId()
        {
            string sConn = PlavyConfig.GetConnectionString("db");
            SqlDb db = new SQLiteDb(sConn);
            var dbFactory = db.DbBuilder.DbFactory;
            var dbOperator = dbFactory.GetDbParamOperator();

            var dblist = db.ExecuteScalar("select max(id) from tb_certs");
            if (dblist == null)
                return -1;
            return dblist;
        }
    }
}
