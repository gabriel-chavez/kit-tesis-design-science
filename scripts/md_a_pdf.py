# -*- coding: utf-8 -*-
"""Markdown -> HTML -> PDF (Edge headless) con portada, contenido y numeracion."""
import os
import re
import subprocess
import sys
import time
import argparse
from pathlib import Path

import markdown
import fitz  # PyMuPDF

EDGE = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

CSS = """
@page { size: Letter; margin: 2.4cm 2.4cm 2.4cm 2.4cm; }
* { box-sizing: border-box; }
body { font-family: "Times New Roman", serif; font-size: 11.5pt; line-height: 1.45;
       text-align: justify; color: #111; margin: 0; }
h1 { font-size: 17pt; text-align: center; page-break-before: always;
     margin: 0 0 14pt 0; line-height: 1.2; }
h2 { font-size: 14pt; margin: 18pt 0 8pt 0; page-break-after: avoid; }
h3 { font-size: 12pt; margin: 14pt 0 6pt 0; page-break-after: avoid; }
h4 { font-size: 11.5pt; margin: 12pt 0 4pt 0; page-break-after: avoid; }
p { margin: 0 0 7pt 0; orphans: 2; widows: 2; }
ul, ol { margin: 0 0 8pt 0; padding-left: 20pt; }
li { margin-bottom: 3pt; }
table { border-collapse: collapse; width: 100%; margin: 8pt 0 10pt 0;
        font-size: 8.2pt; line-height: 1.25; page-break-inside: auto; }
thead { display: table-header-group; }
tr { page-break-inside: avoid; }
th { background: #1F3864; color: #fff; border: 0.5pt solid #8EA9C1;
     padding: 3pt 3pt; text-align: left; vertical-align: middle; }
td { border: 0.5pt solid #A6A6A6; padding: 3pt 3pt; vertical-align: top;
     word-wrap: break-word; }
code { font-family: Consolas, "Courier New", monospace; font-size: 9pt; }
pre { background: #F4F4F4; border: 0.5pt solid #D9D9D9; padding: 6pt;
      white-space: pre-wrap; page-break-inside: avoid; margin: 6pt 0 10pt 0; }
pre code { font-size: 8pt; }
blockquote { border-left: 3pt solid #1F3864; margin: 8pt 0 8pt 14pt;
             padding-left: 8pt; color: #333; font-style: italic; }
img { max-width: 100%; }
hr { border: none; border-top: 0.5pt solid #BFBFBF; margin: 10pt 0; }
.cover { text-align: center; page-break-after: always; padding-top: 70pt; }
.cover .inst { font-size: 22pt; font-weight: bold; color: #1F3864; letter-spacing: 1pt; }
.cover .line { width: 60%; margin: 10pt auto 28pt auto; border-top: 1pt solid #1F3864; }
.cover .kind { font-size: 13pt; font-weight: bold; letter-spacing: 0.5pt; }
.cover .sub { font-size: 12pt; font-style: italic; color: #555; margin-top: 6pt; }
.cover .title { font-size: 21pt; font-weight: bold; line-height: 1.25;
                margin: 34pt 28pt 30pt 28pt; }
.cover .meta { font-size: 12pt; line-height: 1.6; }
.cover .meta b { display: inline-block; width: 90pt; text-align: right; }
.cover .place { margin-top: 46pt; font-size: 12pt; font-weight: bold; }
.toc { page-break-after: always; }
.toc h1 { page-break-before: avoid; text-align: center; }
.toc .h1 { font-weight: bold; margin: 7pt 0 2pt 0; font-size: 11pt; }
.toc .h2 { margin: 1pt 0 1pt 18pt; font-size: 10pt; }
.note { font-size: 9pt; color: #555; font-style: italic; }
.mermaid-label { font-size: 9pt; color: #555; font-style: italic; margin-top: 8pt; }
"""


def portada(kind, sub, titulo, modalidad, autor, tutor, fecha, ciudad):
    return f"""
<div class="cover">
  <div class="inst">ALSIE</div>
  <div class="line"></div>
  <div class="kind">{kind}</div>
  <div class="sub">{sub}</div>
  <div class="title">{titulo}</div>
  <div class="meta">
    <div><b>Modalidad:</b> {modalidad}</div>
    <div><b>Postulante:</b> {autor}</div>
    <div><b>Tutor:</b> {tutor}</div>
  </div>
  <div class="place">{ciudad} — {fecha}</div>
</div>
"""


def construir_html(texto, args):
    # Separar encabezados para el contenido y los marcadores
    encabezados = []
    dentro_codigo = False
    for linea in texto.split("\n"):
        s = linea.strip()
        if s.startswith("```"):
            dentro_codigo = not dentro_codigo
            continue
        if dentro_codigo:
            continue
        m = re.match(r"^(#{1,2})\s+(.+)$", s)
        if m:
            encabezados.append((len(m.group(1)), m.group(2).strip()))

    # Bloques mermaid: etiqueta + codigo
    texto = re.sub(
        r"```mermaid\s*\n(.*?)```",
        lambda m: "**Diagrama.** Representación textual del diagrama incluido en el documento fuente.\n\n```\n"
                  + m.group(1).strip() + "\n```",
        texto, flags=re.S,
    )

    md = markdown.Markdown(extensions=["tables", "fenced_code", "sane_lists"])
    cuerpo = md.convert(texto)

    # Quitar el primer h1 si repite el titulo de portada
    cuerpo = re.sub(r"<h1>.*?</h1>", "", cuerpo, count=1, flags=re.S) if args.quitar_primer_h1 else cuerpo

    toc = ['<div class="toc"><h1>Contenido</h1>']
    for nivel, titulo in encabezados:
        clase = "h1" if nivel == 1 else "h2"
        toc.append(f'<p class="{clase}">{titulo}</p>')
    toc.append('<p class="note">La paginación se indica en el pie de cada página. '
               'El panel de marcadores del PDF permite navegar por los apartados.</p>')
    toc.append("</div>")

    return f"""<!DOCTYPE html>
<html lang="es"><head><meta charset="utf-8"><style>{CSS}</style></head>
<body>
{portada(args.kind, args.subtitulo, args.titulo, args.modalidad, args.autor, args.tutor, args.fecha, args.ciudad)}
{''.join(toc)}
{cuerpo}
</body></html>"""


def agregar_paginas_y_marcadores(pdf_in, pdf_out, encabezados):
    doc = fitz.open(pdf_in)
    n = doc.page_count

    # Marcadores: buscar el titulo en las paginas siguientes
    marcadores = []
    inicio = 0
    for nivel, titulo in encabezados:
        busqueda = titulo[:45]
        encontrado = None
        for pno in range(inicio, n):
            if doc[pno].search_for(busqueda):
                encontrado = pno
                break
        if encontrado is None:
            encontrado = inicio
        marcadores.append([nivel, titulo, encontrado + 1])
        inicio = encontrado

    # Numeracion (se omite la portada)
    for i in range(1, n):
        pagina = doc[i]
        rect = fitz.Rect(40, 748, 572, 770)
        pagina.insert_textbox(
            rect, f"Página {i + 1} de {n}", fontsize=8,
            fontname="helv", align=1, color=(0.35, 0.35, 0.35),
        )

    doc.set_toc(marcadores)
    doc.set_metadata({
        "title": os.path.splitext(os.path.basename(pdf_out))[0],
        "author": "Luis Gabriel Chavez Ramos",
        "subject": "Trabajo original de investigación",
    })
    doc.save(pdf_out, garbage=3, deflate=True)
    doc.close()


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--kind", default="TRABAJO ORIGINAL DE INVESTIGACIÓN")
    parser.add_argument("--subtitulo", default="")
    parser.add_argument("--titulo", required=True)
    parser.add_argument("--modalidad", default="Tesis de maestría en Ingeniería de Software")
    parser.add_argument("--autor", default="Luis Gabriel Chavez Ramos")
    parser.add_argument("--tutor", default="Luis Roberto Pérez Rios, Ph.D.")
    parser.add_argument("--fecha", default="Octubre de 2026")
    parser.add_argument("--ciudad", default="Cochabamba, Bolivia")
    parser.add_argument("--quitar-primer-h1", action="store_true")
    args = parser.parse_args()

    with open(args.input, "r", encoding="utf-8") as f:
        texto = f.read()

    html = construir_html(texto, args)
    base = os.path.abspath(os.path.splitext(args.output)[0])
    os.makedirs(os.path.dirname(args.output) or ".", exist_ok=True)
    html_path = base + ".html"
    pdf_tmp = base + "_tmp.pdf"
    with open(html_path, "w", encoding="utf-8") as f:
        f.write(html)

    url = Path(html_path).resolve().as_uri()
    cmd = [
        EDGE, "--headless=new", "--disable-gpu", "--no-pdf-header-footer",
        f"--print-to-pdf={pdf_tmp}", url,
    ]
    subprocess.run(cmd, check=False, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    for _ in range(20):
        if os.path.exists(pdf_tmp) and os.path.getsize(pdf_tmp) > 10000:
            break
        time.sleep(1)

    if not os.path.exists(pdf_tmp):
        raise RuntimeError("Edge no generó el PDF")

    encabezados = []
    dentro_codigo = False
    for linea in texto.split("\n"):
        s = linea.strip()
        if s.startswith("```"):
            dentro_codigo = not dentro_codigo
            continue
        if dentro_codigo:
            continue
        m = re.match(r"^(#{1,2})\s+(.+)$", s)
        if m:
            encabezados.append((len(m.group(1)), m.group(2).strip()))

    agregar_paginas_y_marcadores(pdf_tmp, args.output, encabezados)
    os.remove(pdf_tmp)
    print(args.output)


if __name__ == "__main__":
    main()
