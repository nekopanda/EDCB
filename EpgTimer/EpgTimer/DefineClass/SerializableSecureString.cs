using System;
using System.Text;
using System.Security;
using System.Security.Cryptography;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace EpgTimer
{
    public class SerializableSecureString : IXmlSerializable
    {
        /// <summary>
        /// XmlSerializer は読み込み時、符号化された文字列として Base64String に格納し、復号
        /// できた場合はそれを SecureString に格納する。復号できなかった場合は生文字列として
        /// 扱いそのまま SecureString に格納し、符号化した文字列を Base64String に格納する。
        /// また書き込み時は符号化した文字列の Base64String のみを出力する。
        /// SecureString と Base64String は互いに同期され、片方が更新されるとそれに合わせて
        /// もう片方も更新される。
        /// </summary>
        private SecureString secureString;
        private string cryptString;

        public SerializableSecureString()
        {
            secureString = new SecureString();
            cryptString = "";
        }
        public SerializableSecureString(SecureString s)
        {
            SecureString = s;
        }
        public SerializableSecureString(string s)
        {
            Base64String = s;
        }

        public SecureString SecureString
        {
            get
            {
                return secureString;
            }
            set
            {
                try
                {
                    // SecureString の中を参照するのはマーシャリングを使うのが正攻法らしい。
                    // ただし ProtectedData を使うために byte[] が必要になり結局ローカルにコピーすることになるので
                    // NetworkCredential から string を取得することにする。
                    // ローカルにコピーをなるべく作りたくないので、気休めでしかないが一括で byte[] を作り出す。
                    secureString = value;
                    cryptString = secureString.Length > 0 ?
                        Convert.ToBase64String(
                            ProtectedData.Protect(
                                Encoding.UTF8.GetBytes(
                                    new NetworkCredential(string.Empty, secureString).Password), 
                                null, 
                                DataProtectionScope.CurrentUser)) : "";
                }
                catch
                {
                    secureString.Clear();
                    cryptString = "";
                }
            }
        }
        public string Base64String
        {
            get
            {
                return cryptString;
            }
            set
            {
                try
                {
                    // 同様、Unprotect した中身のコピーをなるべく作りたくないので
                    // 気休めでしかないが一括で char[] を作り、SecureString に登録する。
                    cryptString = value;
                    secureString.Clear();
                    foreach (char c in Encoding.UTF8.GetChars(ProtectedData.Unprotect(Convert.FromBase64String(cryptString), null, DataProtectionScope.CurrentUser)))
                    {
                        secureString.AppendChar(c);
                    }
                }
                catch
                {
                    // decrypt 出来ない文字列は生パスワードとして扱う
                    SecureString s = new SecureString(); ;
                    foreach (char c in value)
                    {
                        s.AppendChar(c);
                    }
                    // SecureString を設定して Bse64String を作る
                    this.SecureString = s;
                }
            }
        }
        public int Length { get { return secureString.Length; } }
        public HMAC HMAC { get { return new HMACMD5(Encoding.UTF8.GetBytes(new NetworkCredential(string.Empty, secureString).Password)); } }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            bool isEnd = reader.IsEmptyElement;
            while (!isEnd)
            {
                reader.Read();
                if (reader.NodeType == XmlNodeType.Text)
                {
                    Base64String = reader.Value as string;
                }
                isEnd = reader.NodeType == XmlNodeType.EndElement;
            }
            reader.Read();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteValue(Base64String);
        }
    }
}
