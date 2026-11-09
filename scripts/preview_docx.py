# -*- coding: utf-8 -*-
"""Vista previa de un DOCX: mammoth -> HTML -> Edge PDF -> PNG."""
import os
import subprocess
import sys
from pathlib import Path

import mammoth
import pymupdf

EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

CSS = """
<style>
@page { size: Letter; margin: 2.2cm; }
body { font-family: 'Times New Roman', serif; font-size: 11.5pt; line-height: 1.4;
       text-align: justify; }
h1 { text-align: center; font-size: 17pt; page-break-before: always; }
h2 { font-size: 14pt; }
h3 { font-size: 12pt; }
table { border-collapse: collapse; width: 100%; font-size: 8pt; }
td, th { border: 0.5pt solid #999; padding: 2pt; vertical-align: top; }
th { background: #1F3864; color: #fff; }
img { max-width: 100%; }
pre { white-space: pre-wrap; font-size: 8pt; }
</style>
"""


def main():
    docx = sys.argv[1]
    outdir = os.path.join(os.path.dirname(docx), "preview")
    os.makedirs(outdir, exist_ok=True)
    with open(docx, "rb") as f:
        result = mammoth.convert_to_html(f)
    html = ("<!DOCTYPE html><html><head><meta charset='utf-8'>" + CSS +
            "</head><body>" + result.value + "</body></html>")
    html_path = os.path.join(outdir, "preview.html")
    with open(html_path, "w", encoding="utf-8") as f:
        f.write(html)
    pdf = os.path.abspath(os.path.join(outdir, "preview.pdf"))
    url = Path(html_path).resolve().as_uri()
    subprocess.run([EDGE, "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
                    f"--print-to-pdf={pdf}", url], check=False,
                   stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    d = pymupdf.open(pdf)
    print("paginas", d.page_count)
    for i in range(min(d.page_count, 6)):
        d[i].get_pixmap(dpi=100).save(os.path.join(outdir, f"p{i+1}.png"))
    d.close()


if __name__ == "__main__":
    main()
