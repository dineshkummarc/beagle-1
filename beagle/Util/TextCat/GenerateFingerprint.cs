//
// GenerateFingerprint.cs: Program to generate a fingerprint and save it to a file
//
// Copyright 2006 Paul Betts (paul.betts@gmail.com)

//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

using System;
using System.Collections;
using System.IO;
using System.Threading;
using CommandLineFu;
using Beagle.Util;

class GenerateFingerprintTool {

        static int Main (string [] args)
        {
                CommandLine.ProgramName = "beagle-generatefingerprint";
                CommandLine.ProgramCopyright = "Copyright (C) 2006 Paul Betts";
                args = CommandLine.Process (typeof (GenerateFingerprintTool), args);
                if (args == null)
                        return -1;

                for (int i=0; i < args.Length; i++)
                {
                        string input_file = args [i];
                        string output_file = System.IO.Path.ChangeExtension (input_file, ".fp");
                        TextFingerprint fp = new TextFingerprint ();
                        try {
                                StreamWriter sw = new StreamWriter (output_file);
                                fp.Analyze ( new StreamReader (input_file) );
                                fp.Write (sw);
                                sw.Close ();
                        }
                        catch (Exception e) 
                        { 
                                Console.WriteLine ("Error! {0}\nStack Trace: {1}", e.Message, e.StackTrace);
                                return -1;
                        }
                }

                return 0;
        }
}
