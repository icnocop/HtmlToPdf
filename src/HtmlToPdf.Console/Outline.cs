﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 
namespace HtmlToPdf.Console.Outline.Xml {
    using System.Collections.Generic;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://wkhtmltopdf.org/outline")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://wkhtmltopdf.org/outline", IsNullable=false)]
    public partial class item {
        
        private List<item> childrenField;
        
        private string titleField;
        
        private string pageField;
        
        private string linkField;
        
        private string backLinkField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("item")]
        public List<item> children {
            get {
                return this.childrenField;
            }
            set {
                this.childrenField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string title {
            get {
                return this.titleField;
            }
            set {
                this.titleField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string page {
            get {
                return this.pageField;
            }
            set {
                this.pageField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string link {
            get {
                return this.linkField;
            }
            set {
                this.linkField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string backLink {
            get {
                return this.backLinkField;
            }
            set {
                this.backLinkField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://wkhtmltopdf.org/outline")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://wkhtmltopdf.org/outline", IsNullable=false)]
    public partial class outline {
        
        private List<item> itemsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("item")]
        public List<item> Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
    }
}
