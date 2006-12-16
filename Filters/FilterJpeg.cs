//
// FilterJpeg.cs
//
// Copyright (C) 2004 Novell, Inc.
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
using System.IO;
using System.Text;

using Beagle.Util;
using Beagle.Util.Exif;
using Beagle.Util.Xmp;
using Beagle.Daemon;

using SemWeb;

namespace Beagle.Filters {
	
	[PropertyKeywordMapping (Keyword="imagemodel",     PropertyName="exif:Model",    IsKeyword=true)]
	public class FilterJpeg : FilterImage {

		public FilterJpeg () : base ()
		{
			AddSupportedFlavor (FilterFlavor.NewFromMimeType ("image/jpeg"));
		}

		// FIXME: This is not particularly efficient
		protected override void PullImageProperties ()
		{
			JpegHeader header = new JpegHeader (Stream);

			string comment = header.GetJFIFComment ();
			AddProperty (Beagle.Property.New ("jfif:Comment", comment));
			AddProperty (Beagle.Property.NewUnstored ("fixme:comment", comment));

			ExifData exif = header.Exif;
			if (exif == null)
				return;

			string str;
			
			str = exif.LookupFirstValue (ExifTag.UserComment);
			AddProperty (Beagle.Property.New ("exif:UserComment", str));
			AddProperty (Beagle.Property.NewUnstored ("fixme:comment", str));


			str = exif.LookupFirstValue (ExifTag.ImageDescription);
			AddProperty (Beagle.Property.New ("exif:ImageDescription", str));
			AddProperty (Beagle.Property.NewUnstored ("fixme:comment", str));

			str = exif.LookupFirstValue (ExifTag.PixelXDimension);
			if (str != null && str != String.Empty) {
				Width = Int32.Parse (str);
				AddProperty (Beagle.Property.NewUnsearched ("exif:PixelXDimension", str));
			}

			str = exif.LookupFirstValue (ExifTag.PixelYDimension);
			if (str != null && str != String.Empty) {
				Height = Int32.Parse (str);
				AddProperty (Beagle.Property.NewUnsearched ("exif:PixelYDimension", str));
			}

			str = exif.LookupFirstValue (ExifTag.ISOSpeedRatings);
			AddProperty (Beagle.Property.NewUnsearched ("exif:ISOSpeedRatings", str));

			str = exif.LookupFirstValue (ExifTag.ShutterSpeedValue);
			AddProperty (Beagle.Property.NewUnsearched ("exif:ShutterSpeedValue", str));

			str = exif.LookupFirstValue (ExifTag.ExposureTime);
			AddProperty (Beagle.Property.NewUnsearched ("exif:ExposureTime", str));

			str = exif.LookupFirstValue (ExifTag.FNumber);
			AddProperty (Beagle.Property.NewUnsearched ("exif:FNumber", str));

			str = exif.LookupFirstValue (ExifTag.ApertureValue);
			AddProperty (Beagle.Property.NewUnsearched ("exif:ApertureValue", str));

			str = exif.LookupFirstValue (ExifTag.FocalLength);
			AddProperty (Beagle.Property.NewUnsearched ("exif:FocalLength", str));

			str = exif.LookupFirstValue (ExifTag.Flash);
			AddProperty (Beagle.Property.NewUnsearched ("exif:Flash", str));

			str = exif.LookupFirstValue (ExifTag.Model);
			AddProperty (Beagle.Property.NewKeyword ("exif:Model", str));

			str = exif.LookupFirstValue (ExifTag.Copyright);
			AddProperty (Beagle.Property.New ("exif:Copyright", str));

			str = exif.LookupFirstValue (ExifTag.DateTime);
			if (str != null && str != String.Empty) {
				try {
					DateTime dt = ExifUtil.DateTimeFromString (str);
					AddProperty (Beagle.Property.NewDate ("exif:DateTime", dt));
				} catch (ArgumentOutOfRangeException) {
					Logger.Log.Debug("EXIF DateTime '{0}' is invalid.", str);
				}
			}

			XmpFile xmp = header.GetXmp ();
			if (xmp != null)
				AddXmpProperties (xmp);

			Finished (); // That's all folks...
		}
	}
}
