﻿/* 
 Copyright (c) 2010, NHIN Direct Project
 All rights reserved.

 Authors:
    Umesh Madan     umeshma@microsoft.com
  
Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
Neither the name of the The NHIN Direct Project (nhindirect.org). nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using NHINDirect.Certificates;

namespace NHINDirect.Agent.Config
{
    [XmlType("AgentSettings")]
    public class AgentSettings
    {
        CryptographerSettings m_cryptographerSettings;
        
        public AgentSettings()
        {
        }
        
        [XmlElement("Domain", typeof(string))]
        public string[] Domains
        {
            get;
            set;
        }
        
        [XmlElement("PrivateCerts")]
        public CertificateSettings PrivateCerts
        {
            get;
            set;
        }

        [XmlElement("PublicCerts")]
        public CertificateSettings PublicCerts
        {
            get;
            set;
        }
        
        [XmlElement("Anchors")]
        public TrustAnchorSettings Anchors
        {
            get;
            set;
        }   
        
        [XmlElement("Cryptographer")]
        public CryptographerSettings Cryptographer
        {
            get
            {
                if (m_cryptographerSettings == null)
                {
                    m_cryptographerSettings = new CryptographerSettings();
                }
                
                return m_cryptographerSettings;
            }
            set
            {
                m_cryptographerSettings = value;
            }
        }
        
        public virtual void Validate()
        {
            if (!AgentDomains.Validate(this.Domains))
            {
                throw new AgentConfigException(AgentConfigError.InvalidDomainList);
            }
                        
            if (this.PrivateCerts == null)
            {
                throw new AgentConfigException(AgentConfigError.MissingPrivateCertSettings);
            }
            this.PrivateCerts.Validate(AgentConfigError.MissingPrivateCertResolver);
            
            if (this.PublicCerts == null)
            {
                throw new AgentConfigException(AgentConfigError.MissingPublicCertSettings);
            }
            this.PublicCerts.Validate(AgentConfigError.MissingPublicCertResolver);
            
            if (this.Anchors == null)
            {
                throw new AgentConfigException(AgentConfigError.MissingAnchorSettings);
            }
            this.Anchors.Validate();
        }
        
        public NHINDAgent CreateAgent()
        {
            this.Validate();
            
            ICertificateResolver privateCerts = this.PrivateCerts.Resolver.CreateResolver();
            ICertificateResolver publicCerts = this.PublicCerts.Resolver.CreateResolver();
            ITrustAnchorResolver trustAnchors = this.Anchors.Resolver.CreateResolver();

            return new NHINDAgent(this.Domains, privateCerts, publicCerts, trustAnchors, TrustModel.Default, this.Cryptographer.Create());
        }
        
        public static AgentSettings Load(string configXml)
        {
            return Load<AgentSettings>(configXml);
        }

        public static T Load<T>(string configXml)
            where T : AgentSettings
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(configXml))
            {
                return (T) serializer.Deserialize(reader);
            }
        }
        
        public static T LoadFile<T>(string filePath)
            where T : AgentSettings
        {
            using(StreamReader reader = new StreamReader(filePath))
            {
                return Load<T>(reader.ReadToEnd());
            }
        }
    }
}
