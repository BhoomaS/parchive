using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CH.Business
{
	public static class iTextSharpUtils
	{
		public static bool MergePdfFiles(IEnumerable<string> fileNames, string targetPdf)
		{
			bool merged = true;
			using (var stream = new FileStream(targetPdf, FileMode.Create))
			{
				Document document = new Document();
				PdfCopy pdf = new PdfCopy(document, stream);
				PdfReader reader = null;
				try
				{
					document.Open();
					foreach (string file in fileNames)
					{
						reader = new PdfReader(file);
						pdf.AddDocument(reader);
						reader.Close();
					}
				}
				catch (Exception)
				{
					merged = false;
					if (reader != null)
					{
						reader.Close();
					}
				}
				finally
				{
					if (document != null)
					{
						document.Close();
					}
				}
			}
			return merged;
		}

		public static byte[] MergePdfFileBytes(IEnumerable<byte[]> filesBytes)
		{
			bool merged = true;
			using (var stream = new MemoryStream())
			{
				var document = new Document();
				var pdf = new PdfCopy(document, stream);
				PdfReader reader = null;
				try
				{
					document.Open();
					foreach (var fileBytes in filesBytes)
					{
						reader = new PdfReader(fileBytes);
						pdf.AddDocument(reader);
						reader.Close();
					}
				}
				catch (Exception)
				{
					merged = false;
					if (reader != null)
						reader.Close();
				}
				finally
				{
					if (document != null)
						document.Close();
				}

				if (merged)
					return stream.ToArray();

				return null;
			}
		}

	}
}
