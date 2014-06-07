using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UM.Model;
using System.Data.Common;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data;
using S1.CommonBiz;
using SOProvider;

namespace UM.DataContext
{
    public class SecuritySettingDC
    {
        public static List<SecuritySetting> GetListSecuritySetting()
        {
            List<SecuritySetting> objSecuritySettingLIst = new List<SecuritySetting>();
            SecuritySetting objSecuritySetting;
            string sSql = "SELECT SecuritySettingId,MaxInvalidPassAtmpt,ReqChangePassFirstLogin,MinRepeatPassAllowed,PassExpireDay,MinReqPassLen,MinReqNonAlphaNumChar,NonAlphaNumChar FROM SecuritySetting";
            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    using (IDataReader dr = db.ExecuteReader(cmd))
                    {
                        while (dr.Read())
                        {
                            objSecuritySetting = new SecuritySetting();
                            objSecuritySetting.SecuritySettingId_PK = dr["SecuritySettingId"].ToString();
                            objSecuritySetting.MaxInvalidPassAtmpt = Convert.ToInt32(dr["MaxInvalidPassAtmpt"]);
                            objSecuritySetting.ReqChangePassFirstLogin = Convert.ToBoolean(dr["ReqChangePassFirstLogin"]);
                            objSecuritySetting.MinRepeatPassAllowed = Convert.ToInt32(dr["MinRepeatPassAllowed"]);
                            objSecuritySetting.PassExpireDay = Convert.ToInt32(dr["PassExpireDay"]);
                            objSecuritySetting.MinReqPassLen = Convert.ToInt32(dr["MinReqPassLen"]);
                            objSecuritySetting.MinReqNonAlphaNumChar = Convert.ToInt32(dr["MinReqNonAlphaNumChar"]);
                            objSecuritySetting.NonAlphaNumChar = dr["NonAlphaNumChar"].ToString();                     
                            objSecuritySettingLIst.Add(objSecuritySetting);
                        }
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }
            return objSecuritySettingLIst;
        }

        public static string SaveSecuritySetting(SecuritySetting objSecuritySetting)
        {
            string sSql = "UPDATE SecuritySetting SET UserCode=@UserCode ,ActionDate =@ActionDate ,ActionType = @ActionType,  MaxInvalidPassAtmpt = @MaxInvalidPassAtmpt, ReqChangePassFirstLogin = @ReqChangePassFirstLogin, MinRepeatPassAllowed = @MinRepeatPassAllowed, PassExpireDay = @PassExpireDay, MinReqPassLen =@MinReqPassLen, MinReqNonAlphaNumChar =@MinReqNonAlphaNumChar, NonAlphaNumChar = @NonAlphaNumChar WHERE  SecuritySettingId = @SecuritySettingId";           

            Database db = DatabaseFactory.CreateDatabase(DCConstant.ConnectionKey);
            DbCommand cmd = db.GetSqlStringCommand(sSql);
            db.AddInParameter(cmd, "UserCode", DbType.String, SessionUtility.SessionContainer.USER_ID);
            db.AddInParameter(cmd, "ActionDate", DbType.DateTime, DateTime.Now);
            db.AddInParameter(cmd, "ActionType", DbType.String, DBAction.Update);
            db.AddInParameter(cmd, "MaxInvalidPassAtmpt", DbType.Int32, objSecuritySetting.MaxInvalidPassAtmpt);
            db.AddInParameter(cmd, "ReqChangePassFirstLogin", DbType.Decimal, objSecuritySetting.ReqChangePassFirstLogin);
            db.AddInParameter(cmd, "MinRepeatPassAllowed", DbType.Int32, objSecuritySetting.MinRepeatPassAllowed);
            db.AddInParameter(cmd, "PassExpireDay", DbType.Int32, objSecuritySetting.PassExpireDay);
            db.AddInParameter(cmd, "MinReqPassLen", DbType.Int32, objSecuritySetting.MinReqPassLen);
            db.AddInParameter(cmd, "MinReqNonAlphaNumChar", DbType.Int32, objSecuritySetting.MinReqNonAlphaNumChar);
            db.AddInParameter(cmd, "NonAlphaNumChar", DbType.String, objSecuritySetting.NonAlphaNumChar);
            db.AddInParameter(cmd, "SecuritySettingId", DbType.String, objSecuritySetting.SecuritySettingId_PK);
            using (DbConnection cn = db.CreateConnection())
            {
                cn.Open();
                try
                {
                    db.ExecuteNonQuery(cmd);

                }
                catch
                {
                    throw;
                }
                finally
                {
                    cn.Close();
                }
            }
            SOSession.FlushCacheDependency();
            return string.Empty;
        }
    }
}
