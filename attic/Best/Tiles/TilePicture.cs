//
// TilePicture.cs
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
using System.Collections;

using BU = Beagle.Util;

using Gnome;

namespace Beagle.Tile {

	[HitFlavor (Name="Pictures", Rank=500, Emblem="emblem-picture.png", Color="#f5f5fe",
		    Type="File", MimeType="image/*")]
	public class TilePicture : TileFile {
		public TilePicture (Hit _hit) : base (_hit,
						      "template-picture.html")
		{			
		}

		protected override void PopulateTemplate ()
		{
			base.PopulateTemplate ();

			string thumbnail = Thumbnail.PathForUri (BU.StringFu.PathToQuotedFileUri (Hit.Uri.LocalPath), ThumbnailSize.Normal);

			if (File.Exists (thumbnail))
				Template ["Thumbnail"] = Images.GetHtmlSource (thumbnail, Hit.MimeType);
			else
				Template ["Thumbnail"] = Images.GetHtmlSource (Hit.Uri.LocalPath, Hit.MimeType);

			// F-Spot hacks
			string [] properties = Hit.GetProperties ("fspot:Tag");

			if (properties != null) {
				Template ["Tags"] += String.Join (" / ", properties);
			}
		}
	}
}
