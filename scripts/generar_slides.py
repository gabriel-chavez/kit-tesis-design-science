# -*- coding: utf-8 -*-
"""Genera la presentacion de predefensa (16 diapositivas) a partir del guion."""
import os
import re

from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN, MSO_ANCHOR
from pptx.enum.shapes import MSO_SHAPE

BASE = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(BASE, "entrega_alsie", "slides", "Predefensa_ALSIE.pptx")
GUION = os.path.join(BASE, "entrega_alsie", "guion_defensa.md")
IMG_MODELO = os.path.join(BASE, "entrega_alsie", "img", "modelo.png")

AZUL = RGBColor(0x1F, 0x38, 0x64)
AZUL2 = RGBColor(0x2E, 0x54, 0x96)
NARANJA = RGBColor(0xC5, 0x5A, 0x11)
GRIS = RGBColor(0x59, 0x59, 0x59)
GRIS_CLARO = RGBColor(0xF2, 0xF2, 0xF2)
CLARO = RGBColor(0xDE, 0xEA, 0xF6)
BLANCO = RGBColor(0xFF, 0xFF, 0xFF)
VERDE = RGBColor(0x54, 0x82, 0x35)
VERDE_CLARO = RGBColor(0xE2, 0xF0, 0xD9)

W, H = 13.333, 7.5


def notas_por_diapositiva():
    with open(GUION, "r", encoding="utf-8") as f:
        texto = f.read()
    partes = re.split(r"(?=### Diapositiva \d+)", texto)
    notas = {}
    for parte in partes:
        m = re.match(r"### Diapositiva (\d+)", parte)
        if not m:
            continue
        n = int(m.group(1))
        lineas = []
        capturar = False
        for linea in parte.split("\n"):
            s = linea.strip()
            if s.startswith("**Se dice:**"):
                capturar = True
                continue
            if capturar:
                if s.startswith("**Se evita:**") or s.startswith("### "):
                    break
                if s.startswith(">"):
                    lineas.append(s.lstrip("> ").strip())
        notas[n] = " ".join(lineas).strip()
    return notas


def nueva(prs):
    return prs.slides.add_slide(prs.slide_layouts[6])


def caja(slide, l, t, w, h, texto, size=14, bold=False, color=GRIS,
         align=PP_ALIGN.LEFT, fill=None, line=None, shape=MSO_SHAPE.RECTANGLE,
         anchor=MSO_ANCHOR.TOP, italic=False, margin=0.08):
    shp = slide.shapes.add_shape(shape, Inches(l), Inches(t), Inches(w), Inches(h))
    shp.shadow.inherit = False
    if fill is None:
        shp.fill.background()
    else:
        shp.fill.solid()
        shp.fill.fore_color.rgb = fill
    if line is None:
        shp.line.fill.background()
    else:
        shp.line.color.rgb = line
        shp.line.width = Pt(1.0)
    tf = shp.text_frame
    tf.word_wrap = True
    tf.margin_left = Inches(margin)
    tf.margin_right = Inches(margin)
    tf.margin_top = Inches(0.05)
    tf.margin_bottom = Inches(0.05)
    tf.vertical_anchor = anchor
    p = tf.paragraphs[0]
    p.alignment = align
    r = p.add_run()
    r.text = texto
    r.font.name = "Calibri"
    r.font.size = Pt(size)
    r.font.bold = bold
    r.font.italic = italic
    r.font.color.rgb = color
    return shp


def texto(slide, l, t, w, h, contenido, size=16, bold=False, color=GRIS,
          align=PP_ALIGN.LEFT, italic=False, line_spacing=1.0, anchor=MSO_ANCHOR.TOP):
    tb = slide.shapes.add_textbox(Inches(l), Inches(t), Inches(w), Inches(h))
    tf = tb.text_frame
    tf.word_wrap = True
    tf.vertical_anchor = anchor
    tf.margin_left = 0
    tf.margin_right = 0
    tf.margin_top = 0
    tf.margin_bottom = 0
    p = tf.paragraphs[0]
    p.alignment = align
    p.line_spacing = line_spacing
    r = p.add_run()
    r.text = contenido
    r.font.name = "Calibri"
    r.font.size = Pt(size)
    r.font.bold = bold
    r.font.italic = italic
    r.font.color.rgb = color
    return tb


def parrafos(slide, l, t, w, h, items, size=15, color=GRIS, bullet="•",
             espacio=6, bold_first=False):
    tb = slide.shapes.add_textbox(Inches(l), Inches(t), Inches(w), Inches(h))
    tf = tb.text_frame
    tf.word_wrap = True
    tf.margin_left = 0
    tf.margin_right = 0
    tf.margin_top = 0
    tf.margin_bottom = 0
    for i, item in enumerate(items):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.space_after = Pt(espacio)
        p.line_spacing = 1.05
        r = p.add_run()
        r.text = f"{bullet} {item}" if bullet else item
        r.font.name = "Calibri"
        r.font.size = Pt(size)
        r.font.color.rgb = color
        r.font.bold = bold_first and i == 0
    return tb


def encabezado(slide, eyebrow, titulo, numero):
    # barra superior
    barra = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, Inches(W), Inches(0.09))
    barra.fill.solid(); barra.fill.fore_color.rgb = AZUL; barra.line.fill.background()
    texto(slide, 0.6, 0.28, 12.0, 0.3, eyebrow.upper(), size=11, bold=True, color=NARANJA)
    texto(slide, 0.6, 0.55, 12.1, 0.75, titulo, size=27, bold=True, color=AZUL)
    # linea
    ln = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.6), Inches(1.30), Inches(12.1), Inches(0.02))
    ln.fill.solid(); ln.fill.fore_color.rgb = NARANJA; ln.line.fill.background()
    pie(slide, numero)


def pie(slide, numero):
    texto(slide, 0.6, 7.12, 9.5, 0.25,
          "Método de compensación idempotente  ·  Luis Gabriel Chavez Ramos",
          size=9, color=GRIS)
    texto(slide, 11.9, 7.12, 0.85, 0.25, str(numero), size=9, bold=True,
          color=GRIS, align=PP_ALIGN.RIGHT)


def tarjeta(slide, l, t, w, h, titulo, cuerpo, color_titulo=AZUL,
            fill=CLARO, size_t=13, size_c=11, line=None):
    shp = slide.shapes.add_shape(MSO_SHAPE.ROUNDED_RECTANGLE, Inches(l), Inches(t), Inches(w), Inches(h))
    shp.shadow.inherit = False
    shp.fill.solid(); shp.fill.fore_color.rgb = fill
    if line:
        shp.line.color.rgb = line; shp.line.width = Pt(1)
    else:
        shp.line.fill.background()
    tf = shp.text_frame
    tf.word_wrap = True
    tf.margin_left = Inches(0.12); tf.margin_right = Inches(0.12)
    tf.margin_top = Inches(0.08); tf.margin_bottom = Inches(0.06)
    p = tf.paragraphs[0]
    p.alignment = PP_ALIGN.CENTER
    r = p.add_run(); r.text = titulo
    r.font.name = "Calibri"; r.font.size = Pt(size_t); r.font.bold = True
    r.font.color.rgb = color_titulo
    if cuerpo:
        p2 = tf.add_paragraph()
        p2.alignment = PP_ALIGN.CENTER
        p2.space_before = Pt(2)
        r2 = p2.add_run(); r2.text = cuerpo
        r2.font.name = "Calibri"; r2.font.size = Pt(size_c); r2.font.color.rgb = GRIS
    return shp


def tabla(slide, datos, l, t, w, h, size=11, header=True):
    filas, cols = len(datos), len(datos[0])
    shp = slide.shapes.add_table(filas, cols, Inches(l), Inches(t), Inches(w), Inches(h))
    tbl = shp.table
    tbl.first_row = header
    for j, ancho in enumerate([w / cols] * cols):
        tbl.columns[j].width = Emu(int(Inches(ancho)))
    for i, fila in enumerate(datos):
        for j, valor in enumerate(fila):
            cell = tbl.cell(i, j)
            cell.text = ""
            cell.margin_left = Inches(0.05); cell.margin_right = Inches(0.05)
            cell.margin_top = Inches(0.02); cell.margin_bottom = Inches(0.02)
            cell.vertical_anchor = MSO_ANCHOR.MIDDLE
            p = cell.text_frame.paragraphs[0]
            p.alignment = PP_ALIGN.LEFT if j == 0 else PP_ALIGN.CENTER
            r = p.add_run(); r.text = str(valor)
            r.font.name = "Calibri"; r.font.size = Pt(size)
            if i == 0 and header:
                r.font.bold = True; r.font.color.rgb = BLANCO
                cell.fill.solid(); cell.fill.fore_color.rgb = AZUL
            else:
                r.font.color.rgb = RGBColor(0x11, 0x11, 0x11)
                cell.fill.solid()
                cell.fill.fore_color.rgb = BLANCO if i % 2 else GRIS_CLARO
    return tbl


def flecha(slide, x1, y1, x2, y2, color=NARANJA, width=1.6):
    con = slide.shapes.add_connector(1, Inches(x1), Inches(y1), Inches(x2), Inches(y2))
    con.line.color.rgb = color
    con.line.width = Pt(width)


# ------------------------------------------------------------------ slides
def main():
    notas = notas_por_diapositiva()
    prs = Presentation()
    prs.slide_width = Inches(W)
    prs.slide_height = Inches(H)

    # 1. Portada
    s = nueva(prs)
    fondo = s.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, Inches(W), Inches(H))
    fondo.fill.solid(); fondo.fill.fore_color.rgb = AZUL; fondo.line.fill.background()
    barra = s.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.9), Inches(1.45), Inches(1.4), Inches(0.06))
    barra.fill.solid(); barra.fill.fore_color.rgb = NARANJA; barra.line.fill.background()
    texto(s, 0.9, 1.65, 11.5, 0.4, "TESIS DE MAESTRÍA  ·  ALSIE", size=13, bold=True, color=RGBColor(0xF4,0xB1,0x83))
    texto(s, 0.9, 2.05, 11.6, 1.9,
          "Método para el diseño y la verificación de compensaciones idempotentes en transacciones distribuidas basadas en Sagas",
          size=33, bold=True, color=BLANCO, line_spacing=1.05)
    texto(s, 0.9, 4.35, 11.5, 0.4, "Predefensa", size=20, bold=True, color=RGBColor(0xF4,0xB1,0x83))
    texto(s, 0.9, 5.35, 11.5, 1.2,
          "Luis Gabriel Chavez Ramos\nTutor: Luis Roberto Pérez Rios, Ph.D.\nCochabamba, Bolivia — Noviembre de 2026",
          size=14, color=BLANCO, line_spacing=1.25)
    if notas.get(1):
        s.notes_slide.notes_text_frame.text = notas[1]

    # 2. Apertura
    s = nueva(prs)
    encabezado(s, "Apertura · por qué importa", "El problema no es la caída: es no saber si la operación ocurrió", 2)
    datos = [("198.843", "transacciones en 2025"), ("4.302", "errores no controlados"),
             ("226", "tiempos de espera agotados"), ("9,67 h", "resolución media")]
    for i, (num, etiqueta) in enumerate(datos):
        x = 0.6 + i * 3.08
        tarjeta(s, x, 1.65, 2.85, 1.45, num, etiqueta, fill=CLARO, size_t=26, size_c=11)
    texto(s, 0.6, 3.45, 12.1, 0.5,
          "Una operación indeterminada puede ser un cobro sin venta, una venta sin cobro o una compensación ejecutada dos veces.",
          size=17, bold=True, color=NARANJA)
    parrafos(s, 0.6, 4.15, 12.1, 2.4, [
        "Diciembre y enero concentran el 79,59 % del volumen y la mayor parte de los errores.",
        "69 intervenciones manuales y 44 pares duplicados en la base, sin índice único.",
        "El costo no es solo económico: es la confianza en el canal de pago.",
    ], size=15)
    if notas.get(2):
        s.notes_slide.notes_text_frame.text = notas[2]

    # 3. Problema, objeto y campo
    s = nueva(prs)
    encabezado(s, "Perfil de investigación", "Problema → objeto → campo: una sola cadena", 3)
    cadena = [
        ("PROBLEMA", "Indeterminación: no se distingue lo procesado de lo no procesado.", NARANJA),
        ("BRECHA", "No hay un procedimiento evaluado para diseñar y verificar la idempotencia.", AZUL2),
        ("OBJETO", "Gestión de la consistencia de transacciones distribuidas.", AZUL),
        ("CAMPO", "Identidad, estado externo, reserva, indeterminación y verificación.", VERDE),
    ]
    for i, (titulo, cuerpo, color) in enumerate(cadena):
        x = 0.45 + i * 3.16
        caja(s, x, 1.85, 2.85, 1.05, titulo, size=12, bold=True, color=BLANCO,
             fill=color, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        caja(s, x, 2.95, 2.85, 1.55, cuerpo, size=11, color=RGBColor(0x11,0x11,0x11),
             fill=CLARO, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        if i < 3:
            flecha(s, x + 2.87, 2.15, x + 3.14, 2.15)
    texto(s, 0.6, 4.85, 12.1, 0.4,
          "El objeto no es el método: el método es el instrumento con el que se estudia la realidad.",
          size=17, bold=True, color=NARANJA)
    texto(s, 0.6, 5.45, 12.1, 0.9,
          "Delimitación: canal QR, datos de 2025 y período 2026. Fuera de alcance: tarjeta, Tigo Money y conciliación bancaria.",
          size=13, color=GRIS, italic=True)
    if notas.get(3):
        s.notes_slide.notes_text_frame.text = notas[3]

    # 4. Objetivos e hipótesis
    s = nueva(prs)
    encabezado(s, "Perfil de investigación", "Objetivo e hipótesis", 4)
    texto(s, 0.6, 1.55, 5.7, 0.35, "OBJETIVO GENERAL", size=11, bold=True, color=NARANJA)
    texto(s, 0.6, 1.9, 5.7, 1.4,
          "Diseñar y verificar un método para compensaciones idempotentes en Sagas con servicios externos no transaccionales, y fundamentar sus principios.",
          size=14, color=RGBColor(0x11,0x11,0x11))
    parrafos(s, 0.6, 3.35, 5.7, 3.2, [
        "Caracterizar modos de fallo y estados inconsistentes.",
        "Sistematizar los fundamentos teóricos.",
        "Modelar la idempotencia de la compensación.",
        "Elaborar y validar la propuesta.",
    ], size=13, bullet="")
    for i, t in enumerate(["I", "II", "III", "IV"]):
        caja(s, 0.6 + i * 1.45, 6.15, 1.25, 0.5, f"Cap. {t}", size=11, bold=True,
             color=BLANCO, fill=AZUL, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)

    caja(s, 6.65, 1.55, 6.05, 4.9, "", fill=RGBColor(0xFB,0xE5,0xD6), line=NARANJA)
    texto(s, 6.85, 1.7, 5.6, 0.3, "HIPÓTESIS", size=11, bold=True, color=NARANJA)
    texto(s, 6.85, 2.05, 5.6, 2.4,
          "Si se aplica un método estructurado según cinco características esenciales, entonces se eliminan las compensaciones duplicadas y los estados inconsistentes, porque cada repetición encuentra una identidad registrada y cada decisión se toma sobre el estado real del externo.",
          size=12, color=RGBColor(0x11,0x11,0x11))
    caracteristicas = ["Identidad previa", "Estado externo", "Reserva de la compensación", "No asumir", "Verificar el efecto"]
    for i, c in enumerate(caracteristicas):
        caja(s, 6.85 + (i % 2) * 2.85, 4.55 + (i // 2) * 0.62, 2.65, 0.5, c,
             size=10, bold=True, color=AZUL, fill=CLARO, align=PP_ALIGN.CENTER,
             anchor=MSO_ANCHOR.MIDDLE)
    if notas.get(4):
        s.notes_slide.notes_text_frame.text = notas[4]

    # 5. Metodología
    s = nueva(prs)
    encabezado(s, "Diseño metodológico", "Métodos vinculados a objetivos", 5)
    datos = [
        ["Método o técnica", "Objetivo"],
        ["Análisis-síntesis, histórico-lógico, sistémico", "Fundamentar y modelar"],
        ["Mapeo sistemático y revisión bibliográfica", "Delimitar el estado del arte"],
        ["Análisis de documentos, entrevista, observación", "Caracterizar el estado actual"],
        ["Réplica del algoritmo y doble con fallos", "Medir la línea base y validar"],
        ["Pruebas de propiedades y control negativo", "Verificar la idempotencia"],
        ["Walkthrough y sesiones con practicantes", "Medir la aplicación"],
    ]
    tabla(s, datos, 0.6, 1.55, 12.1, 4.2, size=12)
    texto(s, 0.6, 5.95, 12.1, 0.9,
          "Población: operaciones del flujo QR y personal que las opera. Muestra no probabilística y analítica: 69 tickets, 4.302 errores, 14 operaciones sintéticas, 6 escenarios y 2 practicantes.",
          size=12, color=GRIS, italic=True)
    if notas.get(5):
        s.notes_slide.notes_text_frame.text = notas[5]

    # 6. Diagnóstico
    s = nueva(prs)
    encabezado(s, "Diagnóstico · línea base", "El estado actual, con datos", 6)
    datos = [
        ["Indicador", "Línea base"],
        ["Operaciones con estado inconsistente", "~69"],
        ["Errores no controlados", "4.302"],
        ["Tiempos de espera agotados", "226"],
        ["Intervenciones manuales", "69"],
        ["Tiempo de resolución", "9,67 h"],
        ["Pares duplicados en la base", "44 de 892.970"],
    ]
    tabla(s, datos, 0.6, 1.55, 6.3, 3.6, size=12)
    caja(s, 7.25, 1.55, 5.45, 1.6, "", fill=RGBColor(0xFB,0xE5,0xD6), line=NARANJA)
    texto(s, 7.45, 1.68, 5.05, 0.3, "HALLAZGO CENTRAL", size=11, bold=True, color=NARANJA)
    texto(s, 7.45, 2.0, 5.05, 1.0,
          "El sistema no distingue «no procesada» de «procesada con respuesta perdida». Los cuatro casos comparados resultaron indeterminados.",
          size=12, color=RGBColor(0x11,0x11,0x11))
    caja(s, 7.25, 3.4, 5.45, 1.75, "", fill=CLARO)
    texto(s, 7.45, 3.55, 5.05, 0.3, "DOS RUTAS, DOS POLÍTICAS", size=11, bold=True, color=AZUL)
    texto(s, 7.45, 3.9, 5.05, 1.1,
          "Ruta A: no reintenta ante transporte; sí ante error de negocio.\nRuta B: reintenta ante cualquier excepción.",
          size=12, color=RGBColor(0x11,0x11,0x11))
    texto(s, 0.6, 5.45, 12.1, 1.2,
          "La información del estado se pierde en el transporte, el reintento decide a ciegas y la compensación no tiene identidad ni registro.",
          size=16, bold=True, color=NARANJA)
    if notas.get(6):
        s.notes_slide.notes_text_frame.text = notas[6]

    # 7. Marco teórico
    s = nueva(prs)
    encabezado(s, "Marco teórico esencial", "Cuatro fuentes que sostienen el modelo", 7)
    fuentes = [
        ("Sagas", "García-Molina y Salem (1987)", "Compensación en orden inverso"),
        ("Idempotencia", "Helland (2012)", "Reconocer la repetición antes del efecto"),
        ("Consistencia eventual", "Vogels (2009)", "Reconciliar lo indeterminado"),
        ("Verificación", "Hummer et al. (2013); Basiri et al. (2016)", "Comprobar sobre el efecto observable"),
    ]
    for i, (titulo, autor, aporte) in enumerate(fuentes):
        x = 0.45 + i * 3.16
        caja(s, x, 1.7, 2.85, 0.7, titulo, size=14, bold=True, color=BLANCO, fill=AZUL,
             align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        caja(s, x, 2.45, 2.85, 1.9, f"{autor}\n\n{aporte}", size=10.5,
             color=RGBColor(0x11,0x11,0x11), fill=CLARO, align=PP_ALIGN.CENTER,
             anchor=MSO_ANCHOR.MIDDLE)
    caja(s, 0.6, 4.85, 12.1, 1.35, "", fill=RGBColor(0xFB,0xE5,0xD6), line=NARANJA)
    texto(s, 0.85, 5.0, 11.6, 0.35, "TOMA DE POSICIÓN", size=11, bold=True, color=NARANJA)
    texto(s, 0.85, 5.35, 11.6, 0.7,
          "El estado del participante externo es la fuente de verdad, no la memoria del proceso.",
          size=17, bold=True, color=RGBColor(0x11,0x11,0x11))
    if notas.get(7):
        s.notes_slide.notes_text_frame.text = notas[7]

    # 8. Modelo (imagen)
    s = nueva(prs)
    encabezado(s, "Modelo teórico · el corazón", "Cinco componentes y siete relaciones", 8)
    s.shapes.add_picture(IMG_MODELO, Inches(1.37), Inches(1.40), width=Inches(10.6))
    texto(s, 0.6, 6.82, 12.1, 0.3,
          "La idempotencia es una propiedad relacional, no un atributo de una pieza aislada.",
          size=14, bold=True, color=NARANJA, align=PP_ALIGN.CENTER)
    if notas.get(8):
        s.notes_slide.notes_text_frame.text = notas[8]

    # 9. Modelo (relaciones nuevas)
    s = nueva(prs)
    encabezado(s, "Modelo teórico · lo nuevo", "Tres relaciones que explican la repetición", 9)
    tarjetas = [
        ("1", "Identidad de la compensación", "El par (operación, motivo) se registra antes del efecto."),
        ("2", "Decisión con dos hechos", "Categoría de negocio + existencia del recurso."),
        ("3", "Verificación con control negativo", "Sin el componente que garantiza: N efectos; con él: 1."),
    ]
    for i, (num, titulo, cuerpo) in enumerate(tarjetas):
        x = 0.55 + i * 4.15
        caja(s, x, 1.6, 3.85, 0.75, f"{num}. {titulo}", size=14, bold=True, color=BLANCO,
             fill=NARANJA, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        caja(s, x, 2.4, 3.85, 2.2, cuerpo, size=12, color=RGBColor(0x11,0x11,0x11),
             fill=CLARO, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
    texto(s, 0.6, 5.0, 12.1, 1.4,
          "La garantía no depende de la cantidad de reintentos ni de que el externo deduplique: depende de una identidad registrada antes del efecto y del estado real del externo.",
          size=16, bold=True, color=AZUL, align=PP_ALIGN.CENTER)
    texto(s, 0.6, 6.3, 12.1, 0.5,
          "Condición de contorno: la garantía se demuestra en una instancia; entre instancias exige unicidad en la base.",
          size=12, italic=True, color=GRIS, align=PP_ALIGN.CENTER)
    if notas.get(9):
        s.notes_slide.notes_text_frame.text = notas[9]

    # 10. Propuesta
    s = nueva(prs)
    encabezado(s, "Propuesta · definición y estructura", "Un método sobre dos estructuras con autor", 10)
    texto(s, 0.6, 1.55, 12.1, 0.8,
          "Método de compensación idempotente con verificación de estado para Sagas sobre servicios externos no transaccionales.",
          size=15, bold=True, color=AZUL)
    caja(s, 0.6, 2.45, 5.9, 1.0, "Saga · García-Molina y Salem (1987)\nQué se compensa y en qué orden", size=11,
         color=RGBColor(0x11,0x11,0x11), fill=CLARO, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
    caja(s, 6.8, 2.45, 5.9, 1.0, "DSRM · Peffers et al. (2007)\nCómo se construye y se evalúa", size=11,
         color=RGBColor(0x11,0x11,0x11), fill=CLARO, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
    texto(s, 0.6, 3.65, 12.1, 0.35, "NUEVE COMPONENTES · CATORCE DECISIONES", size=11, bold=True, color=NARANJA)
    for i in range(9):
        x = 0.6 + (i % 3) * 3.55
        y = 4.0 + (i // 3) * 0.85
        caja(s, x, y, 3.3, 0.65, f"K{i+1}", size=13, bold=True, color=BLANCO,
             fill=AZUL2 if i % 3 else AZUL, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
    texto(s, 0.6, 6.6, 12.1, 0.35,
          "K1 identidad · K2 bitácora · K3 estado · K4 decisión · K5 compensación · K6 orden · K7 reconciliación · K8 verificación · K9 telemetría",
          size=9, color=GRIS, align=PP_ALIGN.CENTER)
    if notas.get(10):
        s.notes_slide.notes_text_frame.text = notas[10]

    # 11. Propuesta (traza)
    s = nueva(prs)
    encabezado(s, "Propuesta · concreción del modelo", "Del componente del modelo al método", 11)
    datos = [
        ["Componente del modelo", "Elemento de la propuesta"],
        ["Identidad de la operación", "K1"],
        ["Estado del participante externo", "K3 y K4"],
        ["Decisión con dos hechos", "K4"],
        ["Identidad y reserva de la compensación", "K2 y K5"],
        ["Verificación del efecto observable", "K8 y control negativo"],
    ]
    tabla(s, datos, 0.6, 1.6, 7.5, 3.6, size=13)
    caja(s, 8.35, 1.6, 4.35, 1.55, "RESERVA · EJECUCIÓN · CONFIRMACIÓN", size=11, bold=True,
         color=BLANCO, fill=NARANJA, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
    texto(s, 8.35, 3.3, 4.35, 2.0,
          "Primero se registra la intención; después se ejecuta; solo al final se marca aplicada. Registrar después destruye la idempotencia.",
          size=12, color=RGBColor(0x11,0x11,0x11))
    texto(s, 0.6, 5.65, 12.1, 0.9,
          "Catorce pasos; los decisivos: asignar identidad, consultar ante la sospecha, decidir con dos hechos, compensar por el par y verificar el efecto.",
          size=14, bold=True, color=AZUL)
    if notas.get(11):
        s.notes_slide.notes_text_frame.text = notas[11]

    # 12. Demostración
    s = nueva(prs)
    encabezado(s, "Demostración · la propuesta aplicada", "Caso 3147/2026: corte de red después del efecto", 12)
    pasos = ["Identidad", "Bitácora", "Ejecutar", "Consultar estado", "Decidir"]
    for i, p in enumerate(pasos):
        x = 0.5 + i * 2.55
        caja(s, x, 1.65, 2.25, 0.85, f"{i+1}. {p}", size=12, bold=True, color=BLANCO,
             fill=AZUL, align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        if i < 4:
            flecha(s, x + 2.27, 2.07, x + 2.52, 2.07)
    # resultados
    resultados = [("1", "recurso creado"), ("1", "compensación"), ("0", "duplicados")]
    for i, (num, etiqueta) in enumerate(resultados):
        x = 1.4 + i * 3.7
        caja(s, x, 3.1, 3.1, 1.35, num, size=34, bold=True, color=VERDE, fill=VERDE_CLARO,
             align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        texto(s, x, 4.5, 3.1, 0.35, etiqueta, size=13, bold=True, color=GRIS, align=PP_ALIGN.CENTER)
    texto(s, 0.6, 5.2, 12.1, 0.8,
          "El estado real ya existía: el método confirmó la operación en lugar de reprocesar.",
          size=16, bold=True, color=NARANJA, align=PP_ALIGN.CENTER)
    texto(s, 0.6, 6.1, 12.1, 0.7,
          "El practicante independiente resolvió 6 de 8 tareas sin ayuda; sus dos puntos críticos fallidos revelaron ambigüedades que se corrigieron.",
          size=12, italic=True, color=GRIS, align=PP_ALIGN.CENTER)
    if notas.get(12):
        s.notes_slide.notes_text_frame.text = notas[12]

    # 13. Validación
    s = nueva(prs)
    encabezado(s, "Validación · resultados", "12/12 frente a 0/12, sin duplicados", 13)
    datos = [
        ["Métrica", "Método", "Línea base"],
        ["Escenario-caso que cumplen", "12/12", "0/12"],
        ["Recursos duplicados", "0", "10"],
        ["Compensaciones duplicadas", "0", "6"],
        ["Consultas de estado", "6", "0"],
        ["Llamadas externas", "14", "38"],
    ]
    tabla(s, datos, 0.6, 1.55, 8.0, 3.4, size=13)
    caja(s, 8.85, 1.55, 3.85, 1.55, "", fill=RGBColor(0xFB,0xE5,0xD6), line=NARANJA)
    texto(s, 9.0, 1.68, 3.55, 0.3, "CONTROL NEGATIVO", size=10, bold=True, color=NARANJA)
    texto(s, 9.0, 1.98, 3.55, 1.0, "Sin compensador: 3 efectos\nCon compensador: 1", size=12, color=RGBColor(0x11,0x11,0x11))
    caja(s, 8.85, 3.3, 3.85, 1.65, "", fill=CLARO)
    texto(s, 9.0, 3.42, 3.55, 0.3, "ALCANCE", size=10, bold=True, color=AZUL)
    texto(s, 9.0, 3.72, 3.55, 1.1,
          "Una instancia, doble neutro. La idempotencia distribuida depende de la unicidad en la base.",
          size=11, color=RGBColor(0x11,0x11,0x11))
    texto(s, 0.6, 5.25, 12.1, 1.2,
          "Aplicabilidad: evidencia exploratoria con dos participantes. No se alcanzó el criterio y no se reescribió después de ver el resultado.",
          size=15, bold=True, color=NARANJA)
    texto(s, 0.6, 6.35, 12.1, 0.4,
          "Ciclo de mejora: 11 escenarios, 4 defectos corregidos o declarados; versión congelada ec9b8b6…",
          size=11, italic=True, color=GRIS)
    if notas.get(13):
        s.notes_slide.notes_text_frame.text = notas[13]

    # 14. Aporte
    s = nueva(prs)
    encabezado(s, "Aporte teórico y significación", "Cinco principios, no una herramienta", 14)
    principios = [
        ("1", "Identidad previa", "Antes del efecto"),
        ("2", "Estado externo", "Fuente de verdad"),
        ("3", "Reserva", "Antes de compensar"),
        ("4", "No asumir", "Reconsultar y reconciliar"),
        ("5", "Verificar", "Contra el efecto observable"),
    ]
    for i, (num, titulo, sub) in enumerate(principios):
        x = 0.45 + i * 2.52
        caja(s, x, 1.6, 2.3, 0.55, num, size=22, bold=True, color=NARANJA,
             fill=RGBColor(0xFB,0xE5,0xD6), align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        caja(s, x, 2.2, 2.3, 0.75, titulo, size=12, bold=True, color=BLANCO, fill=AZUL,
             align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        texto(s, x, 3.05, 2.3, 0.6, sub, size=10, color=GRIS, align=PP_ALIGN.CENTER)
    caja(s, 0.6, 4.15, 12.1, 1.2, "", fill=CLARO)
    texto(s, 0.8, 4.28, 11.7, 0.3, "SIGNIFICACIÓN PRÁCTICA", size=10, bold=True, color=AZUL)
    texto(s, 0.8, 4.58, 11.7, 0.65,
          "10 recursos y 6 compensaciones duplicadas evitadas; 38 → 14 llamadas externas. Sin afirmar reducción de tiempo ni de costo.",
          size=13, color=RGBColor(0x11,0x11,0x11))
    texto(s, 0.6, 5.65, 12.1, 1.1,
          "Los principios aplican si el externo expone consulta de estado, existe identidad estable y hay acción compensatoria disponible.",
          size=12, italic=True, color=GRIS, align=PP_ALIGN.CENTER)
    if notas.get(14):
        s.notes_slide.notes_text_frame.text = notas[14]

    # 15. Conclusiones
    s = nueva(prs)
    encabezado(s, "Conclusiones y recomendaciones", "Resultados por objetivo", 15)
    conclusiones = [
        ("1", "Se caracterizó el estado actual: 4.302 errores, 69 intervenciones y dos rutas divergentes."),
        ("2", "Se sistematizó la teoría: ningún marco garantiza ni verifica la idempotencia de la compensación."),
        ("3", "Se modeló la idempotencia en cinco componentes y siete relaciones."),
        ("4", "Se validó la propuesta: 12/12, sin duplicados, en el entorno controlado."),
    ]
    for i, (num, texto_c) in enumerate(conclusiones):
        y = 1.55 + i * 0.95
        caja(s, 0.6, y, 0.65, 0.65, num, size=18, bold=True, color=BLANCO, fill=AZUL,
             align=PP_ALIGN.CENTER, anchor=MSO_ANCHOR.MIDDLE)
        texto(s, 1.45, y + 0.05, 11.2, 0.65, texto_c, size=14, color=RGBColor(0x11,0x11,0x11))
    caja(s, 0.6, 5.5, 12.1, 1.1, "", fill=RGBColor(0xFB,0xE5,0xD6), line=NARANJA)
    texto(s, 0.85, 5.62, 11.6, 0.3, "RECOMENDACIÓN PRINCIPAL", size=10, bold=True, color=NARANJA)
    texto(s, 0.85, 5.95, 11.6, 0.55,
          "Aplicar la unicidad en la base y ejecutar la cadena real antes de afirmar idempotencia distribuida.",
          size=13, bold=True, color=RGBColor(0x11,0x11,0x11))
    if notas.get(15):
        s.notes_slide.notes_text_frame.text = notas[15]

    # 16. Cierre
    s = nueva(prs)
    fondo = s.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, Inches(W), Inches(H))
    fondo.fill.solid(); fondo.fill.fore_color.rgb = AZUL; fondo.line.fill.background()
    barra = s.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(0.9), Inches(2.3), Inches(1.4), Inches(0.06))
    barra.fill.solid(); barra.fill.fore_color.rgb = NARANJA; barra.line.fill.background()
    texto(s, 0.9, 2.55, 11.5, 1.0, "Gracias", size=54, bold=True, color=BLANCO)
    texto(s, 0.9, 3.65, 11.5, 0.5, "Quedo atento a las preguntas del tribunal.", size=20, color=BLANCO)
    texto(s, 0.9, 5.4, 11.5, 0.9,
          "Luis Gabriel Chavez Ramos\nTutor: Luis Roberto Pérez Rios, Ph.D.",
          size=13, color=RGBColor(0xD9,0xE2,0xF3), line_spacing=1.2)
    if notas.get(16):
        s.notes_slide.notes_text_frame.text = notas[16]

    os.makedirs(os.path.dirname(OUT), exist_ok=True)
    prs.save(OUT)
    print(OUT)


if __name__ == "__main__":
    main()
