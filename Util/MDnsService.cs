//
// MDnsBrowser.cs
//
// Copyright (C) 2006 Kyle Ambroff <kwa@icculus.org>
//

//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
//


using System;
using System.Collections;
using System.Xml.Serialization;

using Beagle.Util;

namespace Beagle
{
        public class Service
        {
                private ushort port;
                private string name;
                private string uri;
                private string cookie;
                private bool isprotected;
                
                [XmlAttribute ("name")]
                public string Name {
                        get { return name; }
                        set { name = value; }
                }
                
                [XmlAttribute ("uri")]
                public string UriString {
                        get { return uri; }
                        set { uri = value; }
                }

                [XmlAttribute ("password")]
                public bool IsProtected {
                        get { return isprotected; }
                        set { isprotected = value; }
                }

                [XmlAttribute ("cookie")]
                public string Cookie {
                        get { return cookie; }
                        set { cookie = value; }
                }

                public Service (string name, Uri uri, bool isprotected, string cookie)
                {
                        this.name = name;
                        this.uri = UriFu.UriToEscapedString (uri);
                        this.isprotected = isprotected;
                        this.cookie = cookie;
                }

                public Service ()
                {
                        
                }

                public override string ToString ()
                {
                        return String.Format ("{1} ({0})", uri, name);
                }

                public System.Uri GetUri ()
                {
                        return UriFu.EscapedStringToUri (uri);
                }

                public void SetUri (System.Uri u)
                {
                        uri = UriFu.UriToEscapedString (u);
                }
        }
}
