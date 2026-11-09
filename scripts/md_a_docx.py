# -*- coding: utf-8 -*-
"""Conversor Markdown -> DOCX con portada, indice automatico y tablas APA basicas."""
import os
import re
import sys
import argparse

from docx import Document
from docx.shared import Pt, Cm, Inches, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_LINE_SPACING
from docx.enum.table import WD_TABLE_ALIGNMENT
from docx.enum.section import WD_SECTION
from docx.enum.style import WD_STYLE_TYPE
from docx.oxml import OxmlElement
from docx.oxml.ns import qn

AZUL = RGBColor(0x1F, 0x38, 0x64)
GRIS = RGBColor(0x59, 0x59, 0x59)


# ---------------------------------------------------------------- utilidades
def set_cell_shading(cell, hex_color):
    tcPr = cell._tc.get_or_add_tcPr()
    shd = OxmlElement("w:shd")
    shd.set(qn("w:val"), "clear")
    shd.set(qn("w:color"), "auto")
    shd.set(qn("w:fill"), hex_color)
    tcPr.insert_element_before(
        shd, "w:noWrap", "w:tcMar", "w:textDirection",
        "w:tcFitText", "w:vAlign", "w:hideMark",
    )


def set_repeat_table_header(row):
    trPr = row._tr.get_or_add_trPr()
    tblHeader = OxmlElement("w:tblHeader")
    tblHeader.set(qn("w:val"), "true")
    trPr.insert_element_before(
        tblHeader, "w:tblCellSpacing", "w:jc", "w:hidden",
    )


def set_cell_margins(table, top=40, start=70, bottom=40, end=70):
    tblPr = table._tbl.tblPr
    mar = OxmlElement("w:tblCellMar")
    for tag, value in (("top", top), ("start", start), ("bottom", bottom), ("end", end)):
        node = OxmlElement(f"w:{tag}")
        node.set(qn("w:w"), str(value))
        node.set(qn("w:type"), "dxa")
        mar.append(node)
    tblPr.append(mar)


def add_field(paragraph, instr, placeholder=""):
    run = paragraph.add_run()
    fld_begin = OxmlElement("w:fldChar")
    fld_begin.set(qn("w:fldCharType"), "begin")
    instr_el = OxmlElement("w:instrText")
    instr_el.set(qn("xml:space"), "preserve")
    instr_el.text = instr
    fld_sep = OxmlElement("w:fldChar")
    fld_sep.set(qn("w:fldCharType"), "separate")
    txt = OxmlElement("w:t")
    txt.text = placeholder
    fld_end = OxmlElement("w:fldChar")
    fld_end.set(qn("w:fldCharType"), "end")
    run._r.append(fld_begin)
    run._r.append(instr_el)
    run._r.append(fld_sep)
    run._r.append(txt)
    run._r.append(fld_end)
    return run


def set_outline_level(paragraph, level):
    pPr = paragraph._p.get_or_add_pPr()
    out = OxmlElement("w:outlineLvl")
    out.set(qn("w:val"), str(level))
    pPr.insert_element_before(
        out, "w:divId", "w:cnfStyle", "w:rPr", "w:sectPr", "w:pPrChange",
    )


def add_paragraph_border_bottom(paragraph, color="1F3864", size=6):
    pPr = paragraph._p.get_or_add_pPr()
    pbdr = OxmlElement("w:pBdr")
    bottom = OxmlElement("w:bottom")
    bottom.set(qn("w:val"), "single")
    bottom.set(qn("w:sz"), str(size))
    bottom.set(qn("w:space"), "4")
    bottom.set(qn("w:color"), color)
    pbdr.append(bottom)
    pPr.append(pbdr)


# ---------------------------------------------------------------- estilos
def configurar_estilos(doc):
    normal = doc.styles["Normal"]
    normal.font.name = "Times New Roman"
    normal.font.size = Pt(12)
    normal.element.rPr.rFonts.set(qn("w:eastAsia"), "Times New Roman")
    pf = normal.paragraph_format
    pf.line_spacing = 1.5
    pf.space_after = Pt(6)
    pf.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

    estilos = {
        "Heading 1": (16, True, False, WD_ALIGN_PARAGRAPH.CENTER),
        "Heading 2": (14, True, False, WD_ALIGN_PARAGRAPH.LEFT),
        "Heading 3": (12, True, True, WD_ALIGN_PARAGRAPH.LEFT),
        "Heading 4": (12, True, False, WD_ALIGN_PARAGRAPH.LEFT),
    }
    for nombre, (size, bold, italic, align) in estilos.items():
        st = doc.styles[nombre]
        st.font.name = "Times New Roman"
        st.font.size = Pt(size)
        st.font.bold = bold
        st.font.italic = italic
        st.font.color.rgb = RGBColor(0, 0, 0)
        st.paragraph_format.alignment = align
        st.paragraph_format.space_before = Pt(12 if nombre != "Heading 1" else 0)
        st.paragraph_format.space_after = Pt(8)
        st.paragraph_format.keep_with_next = True
        st.paragraph_format.line_spacing = 1.15
        if nombre == "Heading 1":
            st.paragraph_format.page_break_before = True
            st.paragraph_format.space_before = Pt(0)

    # Listas
    for nombre in ("List Bullet", "List Bullet 2", "List Bullet 3"):
        try:
            st = doc.styles[nombre]
            st.font.name = "Times New Roman"
            st.font.size = Pt(12)
            st.paragraph_format.line_spacing = 1.5
            st.paragraph_format.space_after = Pt(3)
        except KeyError:
            pass

    # Titulo del indice: pagina nueva y formato de seccion
    try:
        st = doc.styles["TOC Heading"]
        st.font.name = "Times New Roman"
        st.font.size = Pt(16)
        st.font.bold = True
        st.font.color.rgb = RGBColor(0, 0, 0)
        st.paragraph_format.page_break_before = True
        st.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
    except KeyError:
        pass

    # Estilo para secciones preliminares que no deben aparecer en el indice
    if "Preliminar" not in [s.name for s in doc.styles]:
        st = doc.styles.add_style("Preliminar", WD_STYLE_TYPE.PARAGRAPH)
        st.base_style = doc.styles["Normal"]
        st.font.name = "Times New Roman"
        st.font.size = Pt(16)
        st.font.bold = True
        st.font.color.rgb = RGBColor(0, 0, 0)
        st.paragraph_format.alignment = WD_ALIGN_PARAGRAPH.CENTER
        st.paragraph_format.page_break_before = True
        st.paragraph_format.space_before = Pt(0)
        st.paragraph_format.space_after = Pt(8)
        st.paragraph_format.keep_with_next = True
        st.paragraph_format.line_spacing = 1.15
        pPr = st.element.get_or_add_pPr()
        out = OxmlElement("w:outlineLvl")
        out.set(qn("w:val"), "9")
        pPr.append(out)


def configurar_seccion(doc, encabezado):
    section = doc.sections[0]
    section.page_width = Inches(8.5)
    section.page_height = Inches(11)
    section.top_margin = Cm(2.5)
    section.bottom_margin = Cm(2.5)
    section.left_margin = Cm(2.5)
    section.right_margin = Cm(2.5)
    section.different_first_page_header_footer = True

    # Encabezado
    header = section.header
    hp = header.paragraphs[0]
    hp.alignment = WD_ALIGN_PARAGRAPH.RIGHT
    run = hp.add_run(encabezado)
    run.font.name = "Times New Roman"
    run.font.size = Pt(9)
    run.font.color.rgb = GRIS

    # Pie con pagina X de Y
    footer = section.footer
    fp = footer.paragraphs[0]
    fp.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = fp.add_run("Página ")
    r.font.name = "Times New Roman"
    r.font.size = Pt(9)
    r.font.color.rgb = GRIS
    add_field(fp, "PAGE", "1")
    r = fp.add_run(" de ")
    r.font.name = "Times New Roman"
    r.font.size = Pt(9)
    r.font.color.rgb = GRIS
    add_field(fp, "NUMPAGES", "1")


# ---------------------------------------------------------------- portada
def portada(doc, institucion, titulo, subtitulo, modalidad, autor, tutor, fecha, ciudad):
    for _ in range(3):
        doc.add_paragraph()
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(institucion)
    r.font.name = "Times New Roman"
    r.font.size = Pt(20)
    r.bold = True
    r.font.color.rgb = AZUL

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("_" * 46)
    r.font.color.rgb = AZUL

    for _ in range(3):
        doc.add_paragraph()

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.line_spacing = 1.15
    r = p.add_run(titulo)
    r.font.name = "Times New Roman"
    r.font.size = Pt(20)
    r.bold = True

    if subtitulo:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        r = p.add_run(subtitulo)
        r.font.name = "Times New Roman"
        r.font.size = Pt(13)
        r.italic = True
        r.font.color.rgb = GRIS

    for _ in range(4):
        doc.add_paragraph()

    datos = [
        ("Modalidad", modalidad),
        ("Postulante", autor),
        ("Tutor", tutor),
    ]
    for etiqueta, valor in datos:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(4)
        r = p.add_run(f"{etiqueta}: ")
        r.bold = True
        r.font.size = Pt(12)
        r2 = p.add_run(valor)
        r2.font.size = Pt(12)

    for _ in range(4):
        doc.add_paragraph()

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run(f"{ciudad} — {fecha}")
    r.font.size = Pt(12)
    r.bold = True


# ---------------------------------------------------------------- inline
TOKEN = re.compile(r"(\*\*.+?\*\*|\*[^*]+?\*|`[^`]+`|\[[^\]]+\]\([^)]+\))")


def add_inline(paragraph, texto, base_size=None, base_bold=False, base_italic=False):
    partes = TOKEN.split(texto)
    for parte in partes:
        if not parte:
            continue
        bold = base_bold
        italic = base_italic
        mono = False
        contenido = parte
        if parte.startswith("**") and parte.endswith("**") and len(parte) > 4:
            bold = True
            contenido = parte[2:-2]
        elif parte.startswith("*") and parte.endswith("*") and len(parte) > 2:
            italic = True
            contenido = parte[1:-1]
        elif parte.startswith("`") and parte.endswith("`") and len(parte) > 2:
            mono = True
            contenido = parte[1:-1]
        elif parte.startswith("[") and "](" in parte:
            m = re.match(r"\[([^\]]+)\]\(([^)]+)\)", parte)
            if m:
                contenido = f"{m.group(1)} ({m.group(2)})"
        run = paragraph.add_run(contenido)
        run.bold = bold
        run.italic = italic
        if mono:
            run.font.name = "Consolas"
            run.font.size = Pt((base_size or 12) - 1.5)
        elif base_size:
            run.font.size = Pt(base_size)
    return paragraph


# ---------------------------------------------------------------- tablas
def es_tabla(lineas, i):
    return (i + 1 < len(lineas) and lineas[i].strip().startswith("|")
            and re.match(r"^\s*\|?[\s:\-|]+\|[\s:\-|]*$", lineas[i + 1]))


def dividir_fila(linea):
    s = linea.strip()
    if s.startswith("|"):
        s = s[1:]
    if s.endswith("|"):
        s = s[:-1]
    return [c.strip() for c in s.split("|")]


def agregar_tabla(doc, filas):
    if not filas:
        return
    encabezado = filas[0]
    cuerpo = filas[1:]
    ncols = len(encabezado)
    table = doc.add_table(rows=1, cols=ncols)
    table.style = "Table Grid"
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = True

    hdr = table.rows[0]
    set_repeat_table_header(hdr)
    for j, texto in enumerate(encabezado):
        cell = hdr.cells[j]
        cell.text = ""
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.line_spacing = 1.0
        p.paragraph_format.space_after = Pt(0)
        add_inline(p, texto, base_size=9.5, base_bold=True)
        for run in p.runs:
            run.font.color.rgb = RGBColor(0xFF, 0xFF, 0xFF)
            run.bold = True
        set_cell_shading(cell, "1F3864")

    for fila in cuerpo:
        cells = table.add_row().cells
        for j in range(ncols):
            texto = fila[j] if j < len(fila) else ""
            cell = cells[j]
            cell.text = ""
            p = cell.paragraphs[0]
            p.paragraph_format.line_spacing = 1.0
            p.paragraph_format.space_after = Pt(0)
            p.alignment = WD_ALIGN_PARAGRAPH.LEFT
            add_inline(p, texto, base_size=9.0)

    # espacio posterior
    doc.add_paragraph().paragraph_format.space_after = Pt(2)


# ---------------------------------------------------------------- cuerpo
def agregar_markdown(doc, texto, imagenes_mermaid=None, omitir_hasta=None):
    lineas = texto.replace("\r\n", "\n").replace("\r", "\n").split("\n")
    i = 0
    if omitir_hasta:
        while i < len(lineas) and lineas[i].strip() != omitir_hasta:
            i += 1

    cola_imagenes = list(imagenes_mermaid or [])

    while i < len(lineas):
        linea = lineas[i]
        s = linea.strip()

        if not s:
            i += 1
            continue

        # Marcador de indice automatico
        if s == "<!--TOC-->":
            p = doc.add_paragraph()
            add_field(p, 'TOC \\o "1-2" \\h \\z \\u',
                      "Actualice el campo (F9) para generar el índice.")
            p.paragraph_format.space_after = Pt(12)
            i += 1
            continue

        # Comentarios HTML
        if s.startswith("<!--"):
            i += 1
            continue

        # Regla horizontal
        if re.match(r"^-{3,}$", s):
            i += 1
            continue

        # Bloques de codigo
        if s.startswith("```"):
            lenguaje = s[3:].strip().lower()
            i += 1
            bloque = []
            while i < len(lineas) and not lineas[i].strip().startswith("```"):
                bloque.append(lineas[i])
                i += 1
            i += 1
            if lenguaje == "mermaid" and cola_imagenes:
                ruta = cola_imagenes.pop(0)
                p = doc.add_paragraph()
                p.alignment = WD_ALIGN_PARAGRAPH.CENTER
                p.paragraph_format.space_before = Pt(6)
                p.paragraph_format.space_after = Pt(6)
                p.add_run().add_picture(ruta, width=Inches(6.3))
            else:
                p = doc.add_paragraph()
                p.paragraph_format.left_indent = Cm(0.5)
                p.paragraph_format.line_spacing = 1.0
                p.paragraph_format.space_after = Pt(0)
                r = p.add_run("\n".join(bloque))
                r.font.name = "Consolas"
                r.font.size = Pt(8.5)
            continue

        # Encabezados
        m = re.match(r"^(#{1,4})\s+(.*)$", linea)
        if m:
            nivel = len(m.group(1))
            texto_h = m.group(2).strip()
            preliminares = {
                "Agradecimientos", "Dedicatoria", "Resumen", "Índice",
                "Índice de tablas y figuras",
            }
            if nivel == 1 and texto_h in preliminares:
                p = doc.add_paragraph(style="Preliminar")
                p.paragraph_format.page_break_before = True
            else:
                p = doc.add_paragraph(style=f"Heading {nivel}")
            add_inline(p, texto_h)
            i += 1
            continue

        # Tablas
        if es_tabla(lineas, i):
            filas = [dividir_fila(lineas[i])]
            i += 2
            while i < len(lineas) and lineas[i].strip().startswith("|"):
                filas.append(dividir_fila(lineas[i]))
                i += 1
            agregar_tabla(doc, filas)
            continue

        # Citas
        if s.startswith(">"):
            bloque = []
            while i < len(lineas) and lineas[i].strip().startswith(">"):
                bloque.append(lineas[i].strip()[1:].strip())
                i += 1
            p = doc.add_paragraph()
            p.paragraph_format.left_indent = Cm(1.0)
            p.paragraph_format.right_indent = Cm(0.5)
            p.paragraph_format.line_spacing = 1.15
            p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
            add_inline(p, " ".join(bloque), base_size=11, base_italic=True)
            continue

        # Listas con vineta
        m = re.match(r"^(\s*)[-*]\s+(.*)$", linea)
        if m:
            sangria = len(m.group(1))
            nivel = min(2, sangria // 2)
            estilo = "List Bullet" if nivel == 0 else f"List Bullet {nivel + 1}"
            p = doc.add_paragraph(style=estilo)
            add_inline(p, m.group(2))
            i += 1
            continue

        # Listas numeradas: numero manual para evitar reinicios
        m = re.match(r"^(\s*)(\d+)\.\s+(.*)$", linea)
        if m:
            sangria = len(m.group(1))
            p = doc.add_paragraph()
            p.paragraph_format.left_indent = Cm(0.9 + 0.6 * (sangria // 2))
            p.paragraph_format.first_line_indent = Cm(-0.6)
            p.paragraph_format.space_after = Pt(3)
            add_inline(p, f"{m.group(2)}. {m.group(3)}")
            i += 1
            continue

        # Leyendas de tabla: numero en negrita y titulo en cursiva, en lineas APA
        m = re.match(r"^\*\*(Tabla) (\d+)\*\*$", s)
        if m and i + 1 < len(lineas):
            siguiente = lineas[i + 1].strip()
            if siguiente.startswith("*") and siguiente.endswith("*") and len(siguiente) > 2:
                p = doc.add_paragraph()
                p.paragraph_format.keep_with_next = True
                p.paragraph_format.space_before = Pt(8)
                p.paragraph_format.space_after = Pt(0)
                r = p.add_run(f"{m.group(1)} {m.group(2)}")
                r.bold = True
                r.font.size = Pt(11)
                p2 = doc.add_paragraph()
                p2.paragraph_format.space_after = Pt(4)
                r2 = p2.add_run(siguiente[1:-1])
                r2.italic = True
                r2.font.size = Pt(11)
                i += 2
                continue

        # Parrafo normal
        parrafo = [s]
        i += 1
        while i < len(lineas):
            nxt = lineas[i]
            ns = nxt.strip()
            if (not ns or ns.startswith("#") or ns.startswith("|")
                    or ns.startswith(">") or ns.startswith("```")
                    or re.match(r"^(\s*)[-*]\s+", nxt)
                    or re.match(r"^(\s*)\d+\.\s+", nxt)
                    or re.match(r"^-{3,}$", ns)):
                break
            parrafo.append(ns)
            i += 1
        p = doc.add_paragraph()
        add_inline(p, " ".join(parrafo))


# ---------------------------------------------------------------- main
def convertir(args):
    doc = Document()
    configurar_estilos(doc)
    configurar_seccion(doc, args.encabezado or args.titulo)
    doc.core_properties.title = args.titulo
    doc.core_properties.author = args.autor
    doc.core_properties.subject = args.modalidad

    portada(doc, args.institucion, args.titulo, args.subtitulo,
            args.modalidad, args.autor, args.tutor, args.fecha, args.ciudad)

    with open(args.input, "r", encoding="utf-8") as f:
        texto = f.read()

    if args.indice_automatico:
        texto = re.sub(
            r"## Índice\s*\n.*?(?=## Índice de tablas y figuras)",
            "## Índice\n\n<!--TOC-->\n\n",
            texto, flags=re.S,
        )

    # Normalizar secciones principales del IFI
    texto = re.sub(
        r"^## (Agradecimientos|Dedicatoria|Resumen|Índice|Índice de tablas y figuras|Introducción)\s*$",
        r"# \1", texto, flags=re.M,
    )

    # Las subsecciones de la introduccion pasan a nivel 2
    m = re.search(r"^# Introducción\s*$(.*?)^# Capítulo I", texto, flags=re.M | re.S)
    if m:
        bloque = m.group(1)
        bloque2 = re.sub(r"^### ", "## ", bloque, flags=re.M)
        texto = texto[:m.start(1)] + bloque2 + texto[m.end(1):]

    agregar_markdown(doc, texto, args.mermaid, omitir_hasta=args.omitir_hasta)

    os.makedirs(os.path.dirname(args.output) or ".", exist_ok=True)
    doc.save(args.output)
    print(args.output)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--input", required=True)
    parser.add_argument("--output", required=True)
    parser.add_argument("--institucion", default="ALSIE")
    parser.add_argument("--titulo", required=True)
    parser.add_argument("--subtitulo", default="")
    parser.add_argument("--modalidad", default="Tesis de maestría en Ingeniería de Software")
    parser.add_argument("--autor", default="Luis Gabriel Chavez Ramos")
    parser.add_argument("--tutor", default="Luis Roberto Pérez Rios, Ph.D.")
    parser.add_argument("--fecha", default="Noviembre de 2026")
    parser.add_argument("--ciudad", default="Cochabamba, Bolivia")
    parser.add_argument("--encabezado", default="")
    parser.add_argument("--indice-automatico", action="store_true")
    parser.add_argument("--omitir-hasta", default=None)
    parser.add_argument("--mermaid", nargs="*", default=[])
    args = parser.parse_args()
    convertir(args)
