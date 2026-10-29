"""Generates the README illustrations in Documentation~/images.

Usage: python generate_images.py [output folder]
"""
import math
import os
import sys
from xml.sax.saxutils import escape

OUT = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(os.path.abspath(__file__)), "images")
os.makedirs(OUT, exist_ok=True)

FONT = "'Segoe UI', Inter, -apple-system, BlinkMacSystemFont, 'Helvetica Neue', Arial, sans-serif"
MONO = "'Cascadia Code', Consolas, 'SFMono-Regular', Menlo, monospace"

# Unity dark skin
WIN = "#383838"
TABBAR = "#282828"
TAB = "#3C3C3C"
TOOLBAR = "#3C3C3C"
LINE = "#232323"
TEXT = "#D2D2D2"
SEL = "#2C5D87"
HEADER = "#212121"
BTN = "#585858"
BTN_BORDER = "#2E2E2E"
BTN_TEXT = "#E8E8E8"
FIELD = "#2A2A2A"
FIELD_BORDER = "#1E1E1E"
ON = "#4F4F4F"
# Stage (background around the windows)
STAGE = "#1C2026"
STAGE_BORDER = "#2A3039"
TITLE = "#E6EDF3"
DESC = "#9AA4B2"
ACCENT = "#6CB4FF"
CARD = "#232933"
CARD_BORDER = "#2F3744"


def svg(w, h, body, defs=""):
    return f"""<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">
<defs>
<filter id="shadow" x="-10%" y="-10%" width="120%" height="130%"><feDropShadow dx="0" dy="6" stdDeviation="9" flood-color="#000" flood-opacity="0.45"/></filter>
{defs}
</defs>
<style>
text {{ font-family: {FONT}; }}
.mono {{ font-family: {MONO}; }}
</style>
{body}
</svg>
"""


def t(x, y, s, size=13, fill=TEXT, weight=None, anchor=None, opacity=None, cls=None, ls=None):
    a = f'<text x="{x}" y="{y}" font-size="{size}" fill="{fill}"'
    if weight:
        a += f' font-weight="{weight}"'
    if anchor:
        a += f' text-anchor="{anchor}"'
    if opacity is not None:
        a += f' fill-opacity="{opacity}"'
    if cls:
        a += f' class="{cls}"'
    if ls:
        a += f' letter-spacing="{ls}"'
    return a + f">{escape(s)}</text>"


def r(x, y, w, h, fill, rx=0, stroke=None, sw=1, opacity=None, dash=None):
    a = f'<rect x="{x}" y="{y}" width="{w}" height="{h}" fill="{fill}"'
    if rx:
        a += f' rx="{rx}"'
    if stroke:
        a += f' stroke="{stroke}" stroke-width="{sw}"'
    if opacity is not None:
        a += f' opacity="{opacity}"'
    if dash:
        a += f' stroke-dasharray="{dash}"'
    return a + "/>"


def ln(x1, y1, x2, y2, stroke, sw=1):
    return f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{stroke}" stroke-width="{sw}"/>'


def stage(w, h):
    return r(0.5, 0.5, w - 1, h - 1, STAGE, rx=14, stroke=STAGE_BORDER)


# 16x16 editor icons (simplified versions of Unity's)
ICONS = {
    "go": '<path d="M8 1.5 L14 4.75 V11.25 L8 14.5 L2 11.25 V4.75 Z M2 4.75 L8 8 L14 4.75 M8 8 V14.5" fill="none" stroke="#C8C8C8" stroke-width="1.2" stroke-linejoin="round"/>',
    "prefab": '<path d="M8 1.5 L14 4.75 V11.25 L8 14.5 L2 11.25 V4.75 Z" fill="#4C8DD6"/><path d="M2 4.75 L8 8 L14 4.75 M8 8 V14.5" fill="none" stroke="#A8D0F8" stroke-width="1.1"/>',
    "scene": '<rect x="1.5" y="2.5" width="13" height="11" rx="1.5" fill="none" stroke="#C8C8C8" stroke-width="1.3"/><path d="M3.5 11.5 L6.5 7.5 L8.5 9.5 L10.5 7 L12.5 11.5 Z" fill="#C8C8C8"/>',
    "warn": '<path d="M8 1.5 L15 14 H1 Z" fill="#F4BC02" stroke="#F4BC02" stroke-width="0.8" stroke-linejoin="round"/><rect x="7.2" y="5.5" width="1.6" height="4.8" fill="#2A2A2A"/><rect x="7.2" y="11.3" width="1.6" height="1.6" fill="#2A2A2A"/>',
    "light": '<circle cx="8" cy="8" r="3.1" fill="#F7D44A"/><path d="M8 1v2.2M8 12.8V15M1 8h2.2M12.8 8H15M3 3l1.6 1.6M11.4 11.4 13 13M3 13l1.6-1.6M11.4 4.6 13 3" stroke="#F7D44A" stroke-width="1.3" stroke-linecap="round"/>',
    "terrain": '<path d="M1 13.5 L5.5 6 L8.5 10 L11 7 L15 13.5 Z" fill="#6DBE5A"/>',
    "terrain_col": '<path d="M1.5 13 L5.5 6.5 L8.5 10.5 L11 7.5 L14.5 13 Z" fill="none" stroke="#8BCB5E" stroke-width="1.3" stroke-linejoin="round"/>',
    "camera": '<rect x="1.5" y="5" width="9" height="7" rx="1.2" fill="#B8B8B8"/><path d="M10.5 7.6 L14.5 5.5 V11.5 L10.5 9.4 Z" fill="#B8B8B8"/>',
    "audio": '<path d="M2 6 H5 L9 2.5 V13.5 L5 10 H2 Z" fill="#B8B8B8"/><path d="M11 5.3 Q13.2 8 11 10.7" fill="none" stroke="#B8B8B8" stroke-width="1.3" stroke-linecap="round"/>',
    "script": '<path d="M3 1.5 H10 L13 4.5 V14.5 H3 Z" fill="#E4E4E4"/><path d="M10 1.5 V4.5 H13 Z" fill="#B0B0B0"/><path d="M6.3 7v5.5M8.9 7v5.5M5 8.8h5.2M5 10.9h5.2" stroke="#3A7BD5" stroke-width="1.1"/>',
    "capsule": '<rect x="5" y="1.5" width="6" height="13" rx="3" fill="none" stroke="#8BCB5E" stroke-width="1.4"/>',
    "rigidbody": '<circle cx="8" cy="8" r="5.8" fill="#5B9BD5"/><ellipse cx="6.3" cy="6" rx="2" ry="1.4" fill="#A9D0F5"/>',
    "canvas": '<rect x="1.5" y="2.5" width="13" height="11" rx="1" fill="none" stroke="#5B9BD5" stroke-width="1.4"/><rect x="4" y="5.2" width="8" height="2" fill="#5B9BD5"/><rect x="4" y="8.8" width="5" height="2" fill="#5B9BD5"/>',
    "cursor": '<path d="M4 1.5 L13 9 L8.7 9.6 L11.1 14 L9.4 14.8 L7 10.4 L4 13.2 Z" fill="#B8B8B8"/>',
    "search": '<circle cx="6.8" cy="6.8" r="4.2" fill="none" stroke="#9A9A9A" stroke-width="1.5"/><path d="M10 10 L14 14" stroke="#9A9A9A" stroke-width="1.7" stroke-linecap="round"/>',
    "dots": '<circle cx="8" cy="3.5" r="1.25" fill="#BDBDBD"/><circle cx="8" cy="8" r="1.25" fill="#BDBDBD"/><circle cx="8" cy="12.5" r="1.25" fill="#BDBDBD"/>',
    "arrow_r": '<path d="M6 4.5 L10.5 8 L6 11.5 Z" fill="#B0B0B0"/>',
    "arrow_d": '<path d="M4.5 6 L11.5 6 L8 10.5 Z" fill="#B0B0B0"/>',
    "check": '<path d="M3.5 8.5 L6.8 11.5 L12.5 4.5" fill="none" stroke="#E6E6E6" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>',
    "play": '<path d="M4.5 3 L12.5 8 L4.5 13 Z" fill="#DADADA"/>',
    "refresh": '<path d="M12.8 9 A4.9 4.9 0 1 1 11.2 4.4" fill="none" stroke="#C8C8C8" stroke-width="1.5"/><path d="M9.6 2.2 L13.4 3.9 L10.8 7 Z" fill="#C8C8C8"/>',
    "hier": '<path d="M2 3.5 H11 M6 8 H14 M6 12.5 H14 M3.5 4 V12.5 H6 M3.5 8 H6" fill="none" stroke="#C8C8C8" stroke-width="1.4"/>',
    "dropdown": '<path d="M4.5 6.5 L11.5 6.5 L8 10.5 Z" fill="#C8C8C8"/>',
}


def icon(name, x, y, s=16, opacity=None):
    op = f' opacity="{opacity}"' if opacity is not None else ""
    return f'<g transform="translate({x},{y}) scale({s / 16})"{op}>{ICONS[name]}</g>'


def window(x, y, w, h, title, tab_icon=None):
    out = f'<g filter="url(#shadow)">{r(x, y, w, h, WIN, rx=6, stroke="#161616")}</g>'
    out += f'<path d="M{x} {y + 30} V{y + 6} Q{x} {y} {x + 6} {y} H{x + w - 6} Q{x + w} {y} {x + w} {y + 6} V{y + 30} Z" fill="{TABBAR}"/>'
    tab_w = 30 + len(title) * 7.6 + (20 if tab_icon else 0)
    out += f'<path d="M{x} {y + 30} V{y + 6} Q{x} {y} {x + 6} {y} H{x + tab_w} V{y + 30} Z" fill="{TAB}"/>'
    out += r(x, y, tab_w, 2, "#5A8FCB")
    tx = x + 12
    if tab_icon:
        out += icon(tab_icon, tx, y + 8, 15)
        tx += 21
    out += t(tx, y + 20, title, 13)
    out += icon("dots", x + w - 22, y + 7)
    return out


def toggle(x, y, checked, size=14):
    out = r(x, y, size, size, FIELD, rx=2.5, stroke="#151515")
    if checked:
        out += icon("check", x - 1, y - 1, size + 2)
    return out


def mini_button(x, y, w, h, label=None, ic=None, fill=BTN, disabled=False, rx=3, size=12):
    op = 0.45 if disabled else None
    out = r(x, y, w, h, fill, rx=rx, stroke=BTN_BORDER, opacity=op)
    if label is not None:
        out += t(x + w / 2, y + h / 2 + size * 0.36, label, size, BTN_TEXT, anchor="middle", opacity=op)
    if ic:
        out += icon(ic, x + w / 2 - 7, y + h / 2 - 7, 14, opacity=op)
    return out


def toolbar_button(x, y, w, h, label=None, ic=None, on=False):
    out = ""
    if on:
        out += r(x, y + 1, w, h - 2, ON)
    out += ln(x + w, y + 3, x + w, y + h - 3, LINE)
    if label is not None:
        out += t(x + w / 2, y + h / 2 + 4.5, label, 12, BTN_TEXT, anchor="middle")
    if ic:
        out += icon(ic, x + w / 2 - 7, y + h / 2 - 7, 14)
    return out


def field(x, y, w, h=20, value="", rx=3):
    out = r(x, y, w, h, FIELD, rx=rx, stroke=FIELD_BORDER)
    if value:
        out += t(x + 6, y + h / 2 + 4.5, value, 12.5)
    return out


# ---------------------------------------------------------------- banner

RUNTIME_ACCENT = "#7EE787"

# (label, icon paths on a 24x24 grid, is runtime helper)
BANNER_ICONS = [
    ("Hierarchy", '<path d="M4 5h16M8 12h12M8 19h12M4 5v14h4M4 12h4"/>', False),
    ("Scene Switcher", '<path d="M12 3 21 8 12 13 3 8z"/><path d="M3 12.5 12 17.5 21 12.5"/><path d="M3 16.5 12 21.5 21 16.5"/>', False),
    ("Missing Scripts", '<path d="M12 3.5 21.5 20H2.5z"/><path d="M12 9.5v5M12 17.3v.2"/>', False),
    ("Batch Renamer", '<path d="M4 20h4L19 9l-4-4L4 16z"/><path d="M13 7l4 4"/>', False),
    ("Prefab Replacer", '<path d="M3 7.5 7 5.5l4 2v4l-4 2-4-2z"/><path d="M13 14.5l4-2 4 2v4l-4 2-4-2z"/><path d="M13.5 5.5H18v4.5"/><path d="M10.5 18.5H6V14"/>', False),
    ("Find References", '<circle cx="10.5" cy="10.5" r="6.5"/><path d="M15.5 15.5 21 21"/><path d="M8 10.5h5"/>', False),
    ("Align & Distribute", '<path d="M4 3v18"/><rect x="7" y="5" width="13" height="5" rx="1"/><rect x="7" y="14" width="8" height="5" rx="1"/>', False),
    ("Favorites", '<path d="M12 3.5l2.6 5.3 5.9.9-4.3 4.1 1 5.8L12 16.9l-5.2 2.7 1-5.8-4.3-4.1 5.9-.9z"/>', False),
    ("Drop To Ground", '<path d="M12 3v11"/><path d="M7.5 9.5 12 14l4.5-4.5"/><path d="M3 20h18"/>', False),
    ("Selection History", '<path d="M3.5 12a8.5 8.5 0 1 0 2.5-6"/><path d="M3 4v4.5h4.5"/><path d="M12 7.5V12l3 2"/>', False),
    ("Screenshots", '<rect x="3" y="7" width="18" height="13" rx="2"/><path d="M8.5 7l1.5-3h4l1.5 3"/><circle cx="12" cy="13.5" r="3.5"/>', False),
    ("Scene Auto Save", '<path d="M5 3h11l4 4v14H4V3z"/><path d="M8 3v5h7V3"/><rect x="7.5" y="13" width="9" height="5"/>', False),
    ("Read-Only Field", '<rect x="5" y="11" width="14" height="10" rx="2"/><path d="M8 11V8a4 4 0 0 1 8 0v3"/>', True),
    ("UI Follow", '<rect x="6" y="2.5" width="12" height="5" rx="1.5"/><path d="M12 7.5v3"/><circle cx="12" cy="13.5" r="2.5"/><path d="M8 21v-2a4 4 0 0 1 8 0v2"/>', True),
    ("UI Scroll UV", '<rect x="3" y="5" width="18" height="14" rx="2"/><path d="M7 12h9"/><path d="M13.5 9l3 3-3 3"/>', True),
    ("Beta GUI", '<rect x="3" y="4" width="18" height="16" rx="2"/><path d="M6.5 8.5h7M6.5 12h5"/><path d="M6.5 15.5h3"/>', True),
]


def banner():
    W, H = 1040, 320
    defs = f"""<linearGradient id="bg" x1="0" y1="0" x2="1" y2="1"><stop offset="0" stop-color="#141821"/><stop offset="1" stop-color="#1D2433"/></linearGradient>
<radialGradient id="glow" cx="0.5" cy="0.5" r="0.5"><stop offset="0" stop-color="{ACCENT}" stop-opacity="0.22"/><stop offset="1" stop-color="{ACCENT}" stop-opacity="0"/></radialGradient>
<pattern id="dots" width="22" height="22" patternUnits="userSpaceOnUse"><circle cx="2" cy="2" r="1" fill="#2A3242"/></pattern>
<clipPath id="clip"><rect x="0" y="0" width="{W}" height="{H}" rx="16"/></clipPath>"""
    body = f'<g clip-path="url(#clip)">'
    body += r(0, 0, W, H, "url(#bg)")
    body += r(520, 0, W - 520, H, "url(#dots)")
    body += f'<circle cx="800" cy="150" r="300" fill="url(#glow)"/>'
    body += "</g>"
    body += r(0.5, 0.5, W - 1, H - 1, "none", rx=16, stroke="#2A3242")

    body += t(56, 102, "UNITY PACKAGE", 12.5, ACCENT, weight=700, ls=2.5)
    body += t(54, 156, "BerTaDEV Toolkit", 50, "#F0F4FA", weight=700)
    body += t(56, 196, "Thirteen editor tools and four runtime helpers", 18, "#A7B1C2")
    body += t(56, 221, "that take the everyday friction out of Unity.", 18, "#A7B1C2")
    body += r(56, 242, 392, 34, "#0F131A", rx=17, stroke="#2C3546")
    body += t(76, 264, "com.bertadev.toolkit  ·  v1.2.0  ·  Unity 2022.3+", 13, "#C9D4E5", cls="mono")

    x0, y0, tw, th, gap = 546, 26, 113, 54, 8
    for i, (label, paths, is_runtime) in enumerate(BANNER_ICONS):
        cx = x0 + (i % 4) * (tw + gap)
        cy = y0 + (i // 4) * (th + gap)
        color = RUNTIME_ACCENT if is_runtime else ACCENT
        body += r(cx, cy, tw, th, "#1F2633", rx=9, stroke="#2E3848")
        body += (f'<g transform="translate({cx + tw / 2 - 9.5},{cy + 8}) scale({19 / 24})" fill="none" stroke="{color}" '
                 f'stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round">{paths}</g>')
        body += t(cx + tw / 2, cy + 43, label, 11.5, "#D5DEEA", anchor="middle")

    ly = y0 + 4 * (th + gap) + 14
    body += f'<circle cx="{x0 + 6}" cy="{ly - 4}" r="4" fill="{ACCENT}"/>' + t(x0 + 16, ly, "Editor tools", 12, DESC)
    body += f'<circle cx="{x0 + 106}" cy="{ly - 4}" r="4" fill="{RUNTIME_ACCENT}"/>' + t(x0 + 116, ly, "Runtime helpers (BerTaDEV.Toolkit)", 12, DESC)
    return svg(W, H, body, defs)


# ---------------------------------------------------------------- hierarchy before / after

ROW = 24
HIERARCHY_ROWS = [
    dict(name="--- ENVIRONMENT ---"),
    dict(name="Directional Light", comps=["light"]),
    dict(name="Terrain", comps=["terrain", "terrain_col"]),
    dict(name="Props", children=True, comps=["warn"]),
    dict(name="--- GAMEPLAY ---"),
    dict(name="Player", selected=True, comps=["rigidbody", "capsule", "script"]),
    dict(name="Main Camera", comps=["camera", ("audio", False)]),
    dict(name="Enemy Spawner", active=False, comps=["script", "warn"]),
    dict(name="--- UI ---"),
    dict(name="Canvas", children=True, comps=["canvas", "script", "cursor"]),
    dict(name="EventSystem", comps=["cursor", "script"]),
]


def hierarchy_panel(x, y, w, decorated):
    h = 30 + 28 + 4 + ROW * (len(HIERARCHY_ROWS) + 1) + 8
    out = window(x, y, w, h, "Hierarchy", "hier")
    ty = y + 30
    out += r(x, ty, w, 28, TOOLBAR) + ln(x, ty + 28, x + w, ty + 28, LINE)
    out += mini_button(x + 6, ty + 4, 36, 20)
    out += t(x + 15, ty + 19, "+", 15, BTN_TEXT, weight=600) + icon("dropdown", x + 25, ty + 6, 14)
    out += field(x + 50, ty + 4, w - 58, 20, rx=10) + icon("search", x + 57, ty + 7, 14)

    cy = ty + 28 + 4
    out += r(x, cy, w, ROW, "#3E3E3E")
    out += icon("arrow_d", x + 4, cy + 4) + icon("scene", x + 20, cy + 4) + t(x + 42, cy + 17, "SampleScene", 13.5)
    out += icon("dots", x + w - 22, cy + 4)
    cy += ROW

    for row in HIERARCHY_ROWS:
        name = row["name"]
        active = row.get("active", True)
        selected = row.get("selected", False)
        arrow_x = x + 18
        icon_x = arrow_x + 16
        label_x = icon_x + 21
        if selected:
            out += r(x, cy, w, ROW, SEL)
        if row.get("children"):
            out += icon("arrow_r", arrow_x, cy + 4)

        if decorated and name.startswith("---"):
            out += r(icon_x, cy, x + w - icon_x, ROW, HEADER)
            out += t((icon_x + x + w) / 2, cy + 16.5, name.strip("- "), 13, "#FFFFFF", weight=700, anchor="middle")
            cy += ROW
            continue

        out += icon("go", icon_x, cy + 4, opacity=None if active else 0.45)
        out += t(label_x, cy + 17, name, 13.5, "#FFFFFF" if selected else TEXT, opacity=None if active else 0.45)

        if decorated:
            tx = x + w - 22
            out += toggle(tx, cy + 5, active)
            ix = tx - 1
            for comp in reversed(row.get("comps", [])[-4:]):
                comp_name, enabled = (comp, True) if isinstance(comp, str) else comp
                ix -= 19
                out += icon(comp_name, ix, cy + 4, opacity=None if enabled else 0.35)
        cy += ROW
    return out, h


def hierarchy():
    W, H = 1040, 520
    pw = 450
    body = stage(W, H)
    body += t(40, 36, "Before", 15, DESC, weight=600)
    body += t(550, 36, "With BerTaDEV Toolkit", 15, ACCENT, weight=600)
    left, ph = hierarchy_panel(40, 52, pw, False)
    right, _ = hierarchy_panel(550, 52, pw, True)
    body += left + right
    mid_y = 52 + ph / 2
    body += f'<circle cx="520" cy="{mid_y}" r="15" fill="{CARD}" stroke="{CARD_BORDER}"/>'
    body += f'<path d="M513 {mid_y} H526 M521 {mid_y - 5} L526 {mid_y} L521 {mid_y + 5}" fill="none" stroke="{ACCENT}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>'

    ly = 52 + ph + 34
    items = [
        ("Header rows", "Name a GameObject “--- Name ---”"),
        ("Active toggle", "Toggles the whole selection"),
        ("Component icons", "Dimmed when disabled"),
        ("Missing scripts", "Flagged right in the Hierarchy"),
    ]
    for i, (title, desc) in enumerate(items):
        lx = 40 + i * 240
        if i == 0:
            body += r(lx, ly - 2, 34, 18, HEADER, rx=2) + r(lx + 9, ly + 5, 16, 3, "#FFFFFF", rx=1)
        elif i == 1:
            body += toggle(lx + 10, ly, True)
        elif i == 2:
            body += icon("rigidbody", lx, ly - 1) + icon("capsule", lx + 18, ly - 1) + icon("audio", lx + 36, ly - 1, opacity=0.35)
        else:
            body += icon("warn", lx + 8, ly - 1, 18)
        tx = lx + (58 if i == 2 else 44)
        body += t(tx, ly + 11, title, 14, TITLE, weight=600)
        body += t(lx, ly + 34, desc, 12.5, DESC)
    return svg(W, H, body)


# ---------------------------------------------------------------- scene switcher

def scene_switcher():
    W, H = 520, 340
    x, y, w = 30, 30, 460
    rows = [("Boot", 0), ("Level_01", 2), ("Level_02", 3), ("Level_03", 4), ("MainMenu", 1), ("Sandbox", None), ("TestArena", None)]
    wh = 30 + 28 + 4 + ROW * len(rows) + 30
    body = stage(W, H) + window(x, y, w, wh, "Scenes", "scene")
    ty = y + 30
    body += r(x, ty, w, 28, TOOLBAR) + ln(x, ty + 28, x + w, ty + 28, LINE)
    rx_ = x + w - 30
    body += toolbar_button(rx_, ty, 30, 28, ic="refresh")
    body += toolbar_button(rx_ - 64, ty, 64, 28, "Play #0", on=True)
    body += toolbar_button(rx_ - 64 - 68, ty, 68, 28, "In Build")
    body += ln(rx_ - 64 - 68, ty + 3, rx_ - 64 - 68, ty + 25, LINE)
    body += field(x + 6, ty + 4, rx_ - 64 - 68 - x - 12, 20, rx=10) + icon("search", x + 13, ty + 7, 14)

    cy = ty + 28 + 4
    menu_at = None
    for name, build in rows:
        active = name == "Level_01"
        body += t(x + 10, cy + 17, name, 13.5, "#FFFFFF" if active else TEXT, weight=700 if active else None)
        bx = x + w - 8
        body += mini_button(bx - 28, cy + 3, 28, 18, ic="play", rx=0)
        body += mini_button(bx - 28 - 24, cy + 3, 24, 18, "+", disabled=active, rx=0, size=13)
        body += mini_button(bx - 28 - 24 - 46, cy + 3, 46, 18, "Open", rx=0)
        if build is not None:
            body += t(bx - 98 - 32, cy + 16, f"#{build}", 11, "#9A9A9A")
        if name == "Sandbox":
            menu_at = cy
        cy += ROW

    mx, my = x + 150, menu_at + 16
    items = ["Ping", "Add To Build", "Show In Explorer"]
    body += f'<g filter="url(#shadow)">{r(mx, my, 176, 12 + 24 * len(items), "#2D2D2D", rx=4, stroke="#191919")}</g>'
    for i, item in enumerate(items):
        iy = my + 6 + i * 24
        if i == 1:
            body += r(mx + 4, iy, 168, 24, SEL, rx=3)
        body += t(mx + 22, iy + 16.5, item, 13, "#FFFFFF" if i == 1 else TEXT)
    cx_, cy_ = mx - 4, my - 10
    body += f'<path transform="translate({cx_},{cy_})" d="M0 0 V15 L4 11.5 L6.8 17.5 L9 16.5 L6.3 10.6 H11.5 Z" fill="#FFFFFF" stroke="#111" stroke-width="1" stroke-linejoin="round"/>'
    return svg(W, H, body)


# ---------------------------------------------------------------- missing script finder

def missing_scripts():
    W, H = 520, 340
    x, y, w = 30, 30, 460
    wh = 280
    body = stage(W, H) + window(x, y, w, wh, "Missing Scripts", "warn")
    ty = y + 30
    body += r(x, ty, w, 26, TOOLBAR) + ln(x, ty + 26, x + w, ty + 26, LINE)
    body += toolbar_button(x, ty, 52, 26, "Scan")
    body += toolbar_button(x + 52, ty, 112, 26, "Include Prefabs", on=True)
    body += ln(x + w - 86, ty + 3, x + w - 86, ty + 23, LINE)
    body += toolbar_button(x + w - 86, ty, 86, 26, "Remove All")

    hy = ty + 26 + 6
    body += r(x + 6, hy, w - 12, 46, "#454545", rx=3, stroke="#2B2B2B")
    body += icon("warn", x + 16, hy + 12, 22)
    body += t(x + 48, hy + 19, "6 missing script(s) on 5 GameObject(s).", 12.5)
    body += t(x + 48, hy + 36, "Click a row to select it.", 12.5)

    entries = [
        ("go", "SampleScene > Props", 1),
        ("go", "SampleScene > Enemy Spawner", 1),
        ("prefab", "Barrel > Barrel/Debris", 2),
        ("prefab", "Crate > Crate", 1),
        ("prefab", "Lamp_Post > Lamp_Post/Light", 1),
    ]
    cy = hy + 46 + 8
    for ic, label, count in entries:
        body += icon(ic, x + 10, cy + 3)
        body += t(x + 32, cy + 15.5, label, 13)
        body += t(x + w - 102, cy + 15, f"x{count}", 11, "#9A9A9A")
        body += mini_button(x + w - 70, cy + 2, 62, 18, "Remove", size=11.5)
        cy += 24
    return svg(W, H, body)


# ---------------------------------------------------------------- batch renamer

def batch_renamer():
    W, H = 520, 600
    x, y, w = 30, 30, 460
    wh = 540
    body = stage(W, H) + window(x, y, w, wh, "Batch Renamer")
    fx = x + 150
    fw = x + w - 10 - fx
    cy = y + 30 + 8

    def label(text, indent=0):
        return t(x + 12 + indent * 15, cy + 15, text, 13)

    body += label("Base Name") + field(fx, cy, fw, value="Enemy"); cy += 24
    body += label("Find") + field(fx, cy, fw - 34) + mini_button(x + w - 10 - 28, cy + 1, 28, 18, ".*", size=12); cy += 24
    body += label("Replace With") + field(fx, cy, fw); cy += 24
    body += label("Prefix") + field(fx, cy, fw); cy += 24
    body += label("Suffix") + field(fx, cy, fw); cy += 24
    body += label("Add Numbers") + toggle(fx, cy + 3, True); cy += 24
    body += label("Separator", 1) + field(fx, cy, fw, value="_"); cy += 24
    body += label("Start At", 1) + field(fx, cy, fw, value="1"); cy += 24
    body += label("Step", 1) + field(fx, cy, fw, value="1"); cy += 24
    sw = fw - 56
    body += label("Digits", 1) + r(fx, cy + 9, sw, 3, "#5E5E5E", rx=1.5)
    body += f'<circle cx="{fx + sw * 0.2}" cy="{cy + 10.5}" r="6" fill="#999999" stroke="#2A2A2A"/>'
    body += field(fx + sw + 8, cy, 48, value="2"); cy += 24
    body += label("Case") + r(fx, cy, fw, 20, "#515151", rx=3, stroke="#2E2E2E") + t(fx + 7, cy + 14.5, "Keep", 12.5) + icon("dropdown", fx + fw - 20, cy + 3)
    cy += 24 + 12

    body += t(x + 12, cy + 15, "Preview (5 selected)", 13, TEXT, weight=700); cy += 24
    body += r(x + 8, cy, w - 16, 5 * 22 + 12, "#414141", rx=3, stroke="#2B2B2B")
    half = (w - 60) / 2
    py = cy + 6
    for i in range(5):
        old = "Cube" if i == 0 else f"Cube ({i})"
        body += t(x + 16, py + 15, old, 13)
        body += t(x + 16 + half + 4, py + 15, "→", 13, "#9A9A9A")
        body += t(x + 16 + half + 24, py + 15, f"Enemy_{i + 1:02d}", 13, "#FFFFFF", weight=700)
        py += 22
    cy += 5 * 22 + 12

    by = y + wh - 8 - 30
    body += mini_button(x + 8, by, w - 16, 30, "Rename", size=13.5)
    return svg(W, H, body)


# ---------------------------------------------------------------- shortcut cards

def keycap(x, y, label):
    arrow = label in ("←", "→")
    w = 28 if arrow else 16 + len(label) * 8
    out = r(x, y + 2, w, 26, "#151A21", rx=5)
    out += r(x, y, w, 26, "#2D333B", rx=5, stroke="#444C56")
    if arrow:
        out += t(x + w / 2, y + 18.5, label, 16, TITLE, anchor="middle", weight=700)
    else:
        out += t(x + w / 2, y + 17.5, label, 12.5, TITLE, anchor="middle", cls="mono")
    return out, w


def keys(x, y, combo):
    out = ""
    for i, k in enumerate(combo):
        if i:
            out += t(x + 3, y + 18, "+", 13, DESC)
            x += 14
        cap, w = keycap(x, y, k)
        out += cap
        x += w
    return out, x


def shortcuts():
    W, H = 1040, 300
    body = stage(W, H)
    cw, gap, cy0 = 316, 16, 30
    cards = [
        ("Drop To Ground", [(["End"], "drop"), (["Shift", "End"], "align")], "Lands objects on the collider below them."),
        ("Selection History", [(["Alt", "←"], "back"), (["Alt", "→"], "forward")], "Back and forward through your selections."),
        ("Screenshot", [(["F12"], "capture")], "Saves the Game view at 1x to 4x."),
    ]
    for i, (title, combos, desc) in enumerate(cards):
        cx = 30 + i * (cw + gap)
        body += r(cx, cy0, cw, 240, CARD, rx=12, stroke=CARD_BORDER)
        body += t(cx + 20, cy0 + 34, title, 17, TITLE, weight=700)
        kx = cx + 20
        for combo, what in combos:
            out, kx = keys(kx, cy0 + 50, combo)
            body += out + t(kx + 7, cy0 + 68, what, 12.5, DESC)
            kx += 7 + len(what) * 7 + 18
        body += t(cx + 20, cy0 + 220, desc, 12.5, DESC)

        ix, iy = cx + 20, cy0 + 92
        if i == 0:
            gy = iy + 98
            body += r(ix, gy, 120, 10, "#2F3744") + ln(ix, gy, ix + 120, gy, "#55606F", 1.5)
            body += r(ix + 42, iy, 36, 36, "none", rx=4, stroke=ACCENT, sw=1.5, opacity=0.55, dash="4 3")
            body += f'<path d="M{ix + 60} {iy + 42} V{iy + 56} M{ix + 55} {iy + 51} L{ix + 60} {iy + 56} L{ix + 65} {iy + 51}" fill="none" stroke="{ACCENT}" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" opacity="0.8"/>'
            body += r(ix + 42, gy - 36, 36, 36, ACCENT, rx=4)
            sx0, sy0, sx1, sy1 = ix + 150, gy, ix + 276, gy - 50
            ang = math.degrees(math.atan2(sy1 - sy0, sx1 - sx0))
            body += f'<path d="M{sx0} {sy0} L{sx1} {sy1} V{gy + 10} H{sx0} Z" fill="#2F3744"/>' + ln(sx0, sy0, sx1, sy1, "#55606F", 1.5)
            px = ix + 222
            py = sy0 + (px - sx0) * (sy1 - sy0) / (sx1 - sx0)
            body += r(px - 18, iy - 4, 36, 36, "none", rx=4, stroke=ACCENT, sw=1.5, opacity=0.55, dash="4 3")
            body += f'<g transform="translate({px},{py}) rotate({ang})">{r(-18, -36, 36, 36, ACCENT, rx=4)}</g>'
        elif i == 1:
            chips = ["Player", "Camera", "Canvas"]
            chip_w, chip_gap = 74, 27
            chip_y = iy + 46
            for j, chip in enumerate(chips):
                chx = ix + j * (chip_w + chip_gap)
                current = j == 1
                body += r(chx, chip_y, chip_w, 30, ACCENT if current else "#2D333B", rx=15, stroke=None if current else "#444C56")
                body += t(chx + chip_w / 2, chip_y + 19.5, chip, 12.5, "#0D1117" if current else TITLE, anchor="middle", weight=600 if current else None)
                if j < 2:
                    ax = chx + chip_w + 6
                    body += f'<path d="M{ax} {chip_y + 15} H{ax + chip_gap - 12} M{ax + chip_gap - 16} {chip_y + 11} L{ax + chip_gap - 12} {chip_y + 15} L{ax + chip_gap - 16} {chip_y + 19}" fill="none" stroke="#55606F" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round"/>'
            x_from = ix + 2 * (chip_w + chip_gap) + chip_w / 2
            x_to = ix + (chip_w + chip_gap) + chip_w / 2 + 8
            body += f'<path d="M{x_from} {chip_y - 6} C{x_from} {chip_y - 40} {x_to} {chip_y - 40} {x_to} {chip_y - 9}" fill="none" stroke="{ACCENT}" stroke-width="1.8" stroke-linecap="round"/>'
            body += f'<path d="M{x_to - 5} {chip_y - 15} L{x_to} {chip_y - 8} L{x_to + 5} {chip_y - 15}" fill="none" stroke="{ACCENT}" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>'
            body += t((x_from + x_to) / 2, chip_y - 36, "back", 12, ACCENT, anchor="middle")
        else:
            gx, gy = ix, iy + 8
            body += r(gx, gy, 112, 80, "#2D333B", rx=6, stroke="#444C56")
            body += r(gx, gy, 112, 16, "#262C34", rx=6) + r(gx, gy + 10, 112, 6, "#262C34") + t(gx + 8, gy + 12, "Game", 9.5, DESC)
            body += r(gx + 6, gy + 20, 100, 54, "#3B5B7E")
            body += r(gx + 6, gy + 56, 100, 18, "#4C7A3F")
            body += f'<circle cx="{gx + 84}" cy="{gy + 34}" r="7" fill="#F7D44A"/>'
            body += f'<path d="M{gx + 124} {gy + 40} H{gx + 146} M{gx + 141} {gy + 35} L{gx + 146} {gy + 40} L{gx + 141} {gy + 45}" fill="none" stroke="{ACCENT}" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"/>'
            fx_, fy_ = gx + 156, gy + 2
            body += f'<path d="M{fx_} {fy_ + 4} Q{fx_} {fy_} {fx_ + 4} {fy_} H{fx_ + 18} L{fx_ + 22} {fy_ + 5} H{fx_ + 36} Q{fx_ + 40} {fy_ + 5} {fx_ + 40} {fy_ + 9} V{fy_ + 26} H{fx_} Z" fill="#C9A227"/>'
            body += t(fx_ + 46, fy_ + 18, "Screenshots/", 12, TITLE, cls="mono")
            body += r(fx_, fy_ + 36, 120, 44, "#2D333B", rx=6, stroke="#444C56")
            body += t(fx_ + 8, fy_ + 54, "MyGame_2026-10-30", 10.5, TITLE, cls="mono")
            body += t(fx_ + 8, fy_ + 70, "_14-02-11@2x.png", 10.5, ACCENT, cls="mono")
    return svg(W, H, body)


# ---------------------------------------------------------------- runtime helpers

def game_view(x, y, w, h, sky_top, sky_bottom, ground, grad_id):
    defs = f'<linearGradient id="{grad_id}" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="{sky_top}"/><stop offset="1" stop-color="{sky_bottom}"/></linearGradient>'
    out = r(x, y, w, h, "#2D333B", rx=6, stroke="#444C56")
    out += r(x + 1, y + 1, w - 2, 16, "#262C34", rx=5) + r(x + 1, y + 10, w - 2, 7, "#262C34") + t(x + 9, y + 13, "Game", 9.5, DESC)
    out += r(x + 6, y + 21, w - 12, h - 27, f"url(#{grad_id})")
    out += r(x + 6, y + h - 46, w - 12, 40, ground)
    return out, defs


def runtime():
    W, H = 1040, 556
    body = stage(W, H)
    defs = ""
    cw, ch, gap = 482, 240, 16
    cards = [
        ("[ReadOnlyInspector]", "Shows the value in the Inspector, but locks the field.", "mono"),
        ("UI Follow To World", "Health bars and name tags that stay on their target.", None),
        ("Beta GUI", "Name, version, FPS and build number on screen.", None),
        ("UI Scroll UV", "Endless scrolling RawImage, tiled at the texture's size.", None),
    ]
    for i, (title, desc, cls) in enumerate(cards):
        cx = 30 + (i % 2) * (cw + gap)
        cy0 = 30 + (i // 2) * (ch + gap)
        body += r(cx, cy0, cw, ch, CARD, rx=12, stroke=CARD_BORDER)
        body += t(cx + 20, cy0 + 34, title, 17 if cls is None else 16, TITLE, weight=700, cls=cls)
        body += t(cx + 20, cy0 + 222, desc, 12.5, DESC)
        x, y, w = cx + 20, cy0 + 50, cw - 40

        if i == 0:
            body += r(x, y, w, 104, WIN, rx=4, stroke="#1E1E1E")
            body += r(x + 1, y + 1, w - 2, 24, "#3E3E3E", rx=3)
            body += icon("arrow_d", x + 4, y + 5) + toggle(x + 20, y + 6, True, 13) + icon("script", x + 39, y + 5)
            body += t(x + 60, y + 17.5, "Player Health (Script)", 12.5, TEXT, weight=600) + icon("dots", x + w - 20, y + 5)
            fields = [("Max Health", "100", True), ("Current Health", "73", False), ("Is Dead", None, False)]
            fy = y + 30
            for label, value, editable in fields:
                op = None if editable else 0.45
                body += t(x + 14, fy + 14, label, 12.5, TEXT, opacity=op)
                if value is None:
                    body += f'<g opacity="{op}">{toggle(x + 128, fy + 2, False, 14)}</g>'
                else:
                    body += f'<g opacity="{op if op else 1}">{field(x + 128, fy, w - 136, 19, value)}</g>'
                fy += 24
            body += t(x, y + 128, "[SerializeField, ReadOnlyInspector]", 11.5, ACCENT, cls="mono")
            body += t(x, y + 145, "int currentHealth;", 11.5, TITLE, cls="mono")
        elif i == 1:
            view, d = game_view(x, y, w, 150, "#35587F", "#6A93BC", "#4C7A3F", "sky1")
            body += view
            defs += d
            ground_y = y + 150 - 46
            px = x + 170
            body += r(px, ground_y - 40, 20, 40, "#D9DEE5", rx=10)
            body += t(px + 10, ground_y - 71, "Player", 10.5, "#FFFFFF", anchor="middle", weight=600)
            body += r(px - 20, ground_y - 66, 60, 7, "#14181D", rx=2) + r(px - 19, ground_y - 65, 41, 5, "#57C26A", rx=1.5)
            body += f'<path d="M{px + 10} {ground_y - 58} V{ground_y - 43}" stroke="{ACCENT}" stroke-width="1.4" stroke-dasharray="3 3"/>'
            body += t(px + 16, ground_y - 47, "offset", 10, ACCENT)
            for ex in (x + 320, x + 372):
                body += r(ex, ground_y - 24, 12, 24, "#C98A8A", rx=6)
                body += r(ex - 9, ground_y - 33, 30, 5, "#14181D", rx=1.5) + r(ex - 8, ground_y - 32, 12 if ex < x + 350 else 24, 3, "#E5534B", rx=1)
        elif i == 3:
            tile = 48
            defs += (f'<pattern id="scrolltile" width="{tile}" height="{tile}" patternUnits="userSpaceOnUse">'
                     f'<rect width="{tile}" height="{tile}" fill="#26324A"/>'
                     f'<path d="M-12 12 L12 -12 M0 {tile} L{tile} 0 M{tile - 12} {tile + 12} L{tile + 12} {tile - 12}" stroke="#30405E" stroke-width="12"/>'
                     f'<circle cx="{tile / 2}" cy="{tile / 2}" r="5" fill="{ACCENT}" fill-opacity="0.85"/>'
                     f'<animateTransform attributeName="patternTransform" type="translate" from="0 0" to="-{tile} 0" dur="2.4s" repeatCount="indefinite"/>'
                     f'</pattern>')
            body += r(x, y, w, 150, "#2D333B", rx=6, stroke="#444C56")
            body += r(x + 1, y + 1, w - 2, 16, "#262C34", rx=5) + r(x + 1, y + 10, w - 2, 7, "#262C34") + t(x + 9, y + 13, "Game", 9.5, DESC)
            body += r(x + 6, y + 21, w - 12, 123, "url(#scrolltile)")
            body += r(x + w / 2 - 92, y + 64, 184, 38, "#0F131A", rx=19, opacity=0.85)
            body += f'<path d="M{x + w / 2 - 72} {y + 83} H{x + w / 2 - 46} M{x + w / 2 - 66} {y + 77} L{x + w / 2 - 72} {y + 83} L{x + w / 2 - 66} {y + 89}" fill="none" stroke="{ACCENT}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>'
            body += t(x + w / 2 - 36, y + 88, "speed (0.25, 0)", 13, TITLE, cls="mono")
        else:
            view, d = game_view(x, y, w, 150, "#A9C9E8", "#DCEAF6", "#7FB069", "sky2")
            body += view
            defs += d
            body += f'<circle cx="{x + w - 40}" cy="{y + 50}" r="13" fill="#F7D44A"/>'
            body += f'<path d="M{x + 6} {y + 104} Q{x + 80} {y + 78} {x + 150} {y + 104} T{x + w - 6} {y + 100} V{y + 144} H{x + 6} Z" fill="#94C27C"/>'
            lines = [("MyGame By:BerTaDEV", "#000000"), ("Version: 0.4.2", "#000000"), ("FPS : 144", "#E01E1E"), ("build : 37", "#E01E1E")]
            for j, (text, color) in enumerate(lines):
                body += t(x + 14, y + 38 + j * 15, text, 11.5, color)
    return svg(W, H, body, defs)


# ---------------------------------------------------------------- prefab replacer, favorites, find references, align

ICONS.update({
    "material": '<circle cx="8" cy="8" r="6" fill="#C46A4E"/><ellipse cx="6" cy="6" rx="2.2" ry="1.5" fill="#F0B9A6"/>',
    "folder": '<path d="M1.5 4 Q1.5 3 2.5 3 H6.5 L8 4.5 H13.5 Q14.5 4.5 14.5 5.5 V12.5 Q14.5 13.5 13.5 13.5 H2.5 Q1.5 13.5 1.5 12.5 Z" fill="#C9A227"/>',
    "picker": '<circle cx="8" cy="8" r="4.5" fill="none" stroke="#C8C8C8" stroke-width="1.3"/><circle cx="8" cy="8" r="1.4" fill="#C8C8C8"/>',
    "minus": '<rect x="3.5" y="7" width="9" height="2" rx="1" fill="#BDBDBD"/>',
    "info": '<circle cx="8" cy="8" r="7" fill="#3F7FD0"/><rect x="7.2" y="7" width="1.6" height="5" fill="#FFFFFF"/><rect x="7.2" y="4" width="1.6" height="1.6" fill="#FFFFFF"/>',
    "star": '<path d="M8 1.5 9.9 5.6 14.3 6.1 11 9.1 11.9 13.5 8 11.3 4.1 13.5 5 9.1 1.7 6.1 6.1 5.6Z" fill="#F4BC02"/>',
})


def object_field(x, y, w, ic, name):
    out = field(x, y, w, 20)
    out += icon(ic, x + 4, y + 2) + t(x + 24, y + 14.5, name, 12.5)
    out += r(x + w - 20, y + 1, 19, 18, "#3A3A3A", rx=2) + icon("picker", x + w - 19, y + 2)
    return out


def helpbox(x, y, w, lines, ic="info"):
    h = 16 + 16 * len(lines)
    out = r(x, y, w, h, "#454545", rx=3, stroke="#2B2B2B")
    if ic:
        out += icon(ic, x + 10, y + h / 2 - 9, 18)
    tx = x + (38 if ic else 10)
    for i, line in enumerate(lines):
        out += t(tx, y + 20 + i * 16, line, 12)
    return out, h


def toggle_row(x, fx, y, label, checked):
    return t(x + 12, y + 15, label, 13) + toggle(fx, y + 3, checked)


def prefab_replacer():
    W, H = 520, 440
    x, y, w, wh = 30, 30, 460, 380
    body = stage(W, H) + window(x, y, w, wh, "Prefab Replacer", "prefab")
    fx = x + 150
    fw = x + w - 10 - fx
    cy = y + 38
    body += t(x + 12, cy + 15, "Prefab", 13) + object_field(fx, cy, fw, "prefab", "Tree_Oak"); cy += 24
    body += toggle_row(x, fx, cy, "Keep Rotation", True); cy += 24
    body += toggle_row(x, fx, cy, "Keep Scale", True); cy += 24
    body += toggle_row(x, fx, cy, "Keep Name", False); cy += 24 + 10
    box, bh = helpbox(x + 6, cy, w - 12, ["Each selected object becomes an instance of 'Tree_Oak'",
                                          "at the same spot in the Hierarchy. Their children are",
                                          "removed (Undo brings them back)."])
    body += box
    cy += bh + 8
    body += t(x + 12, cy + 15, "Selected (6)", 13, TEXT, weight=700); cy += 24
    for i in range(4):
        name = "Placeholder_Tree" + ("" if i == 0 else f" ({i})")
        body += icon("go", x + 12, cy + 2) + t(x + 34, cy + 14.5, name, 12.5)
        cy += 21
    body += mini_button(x + 8, y + wh - 38, w - 16, 28, "Replace 6 Object(s)", size=13.5)
    return svg(W, H, body)


def favorites():
    W, H = 520, 440
    x, y, w, wh = 30, 30, 460, 380
    body = stage(W, H) + window(x, y, w, wh, "Favorites", "star")
    ty = y + 30
    body += r(x, ty, w, 26, TOOLBAR) + ln(x, ty + 26, x + w, ty + 26, LINE)
    body += toolbar_button(x, ty, 96, 26, "Add Selection")
    body += ln(x + w - 54, ty + 3, x + w - 54, ty + 23, LINE) + toolbar_button(x + w - 54, ty, 54, 26, "Clear")

    rows = [("prefab", "Player"), ("scene", "MainMenu"), ("material", "M_Ground"), ("script", "PlayerController"),
            ("folder", "Audio"), ("scene", "Boot"), ("prefab", "Tree_Oak"), ("material", "M_Water")]
    cy = ty + 26 + 4
    for ic, name in rows:
        if name == "M_Ground":
            body += r(x, cy, w, 22, SEL)
        body += icon(ic, x + 8, cy + 3) + t(x + 30, cy + 15.5, name, 13, "#FFFFFF" if name == "M_Ground" else TEXT)
        body += icon("minus", x + w - 26, cy + 3)
        cy += 22

    # An asset being dragged in from the Project window.
    body += f'<path d="M{x + 8} {cy + 4} H{x + w - 8}" stroke="{ACCENT}" stroke-width="2" stroke-dasharray="5 4"/>'
    chip_x, chip_y = x + 150, cy + 16
    body += f'<g filter="url(#shadow)">{r(chip_x, chip_y, 150, 26, "#2D2D2D", rx=4, stroke="#191919")}</g>'
    body += icon("prefab", chip_x + 8, chip_y + 5) + t(chip_x + 30, chip_y + 17.5, "Enemy_Spawner", 12.5)
    body += f'<circle cx="{chip_x + 150}" cy="{chip_y}" r="8" fill="#3FB950"/><path d="M{chip_x + 146} {chip_y} H{chip_x + 154} M{chip_x + 150} {chip_y - 4} V{chip_y + 4}" stroke="#FFFFFF" stroke-width="1.8"/>'
    body += f'<path transform="translate({chip_x + 120},{chip_y + 14})" d="M0 0 V15 L4 11.5 L6.8 17.5 L9 16.5 L6.3 10.6 H11.5 Z" fill="#FFFFFF" stroke="#111" stroke-width="1" stroke-linejoin="round"/>'

    body += t(x + w / 2, y + wh - 12, "Drag assets in · double-click to open · drag out to use", 11, "#8A8A8A", anchor="middle")
    return svg(W, H, body)


def find_references():
    W, H = 520, 440
    x, y, w, wh = 30, 30, 460, 380
    body = stage(W, H) + window(x, y, w, wh, "Find References", "search")
    fx = x + 150
    cy = y + 38
    body += t(x + 12, cy + 15, "Asset", 13) + object_field(fx, cy, x + w - 10 - fx, "material", "M_Ground"); cy += 26
    body += mini_button(x + 8, cy, w - 16 - 108, 24, "Find References", size=12.5)
    body += mini_button(x + w - 8 - 100, cy, 100, 24, "Use Selection", size=12.5); cy += 34

    body += t(x + 12, cy + 15, "Used by 4 asset(s)", 13, TEXT, weight=700); cy += 22
    for ic, path in [("prefab", "Assets/Prefabs/Floor.prefab"), ("prefab", "Assets/Prefabs/Ramp.prefab"),
                     ("scene", "Assets/Scenes/Level_01.unity"), ("scene", "Assets/Scenes/Level_02.unity")]:
        body += icon(ic, x + 12, cy + 2) + t(x + 34, cy + 14.5, path, 12.5)
        cy += 21
    cy += 10
    body += t(x + 12, cy + 15, "In open scenes (2)", 13, TEXT, weight=700); cy += 22
    for label in ["Level_01 > Ground (MeshRenderer)", "Level_01 > Props/Platform_02 (MeshRenderer)"]:
        body += icon("go", x + 12, cy + 2) + t(x + 34, cy + 14.5, label, 12.5)
        cy += 21

    fy = y + wh - 28
    body += t(x + 10, fy + 14, "1284 assets indexed", 11, "#9A9A9A")
    body += mini_button(x + w - 8 - 92, fy, 92, 18, "Rebuild Index", size=11)
    return svg(W, H, body)


def align_distribute():
    W, H = 520, 440
    x, y, w, wh = 30, 30, 460, 222
    body = stage(W, H) + window(x, y, w, wh, "Align & Distribute")
    fx = x + 150
    fw = x + w - 10 - fx
    cy = y + 38
    body += t(x + 12, cy + 15, "Relative To", 13) + r(fx, cy, fw, 20, "#515151", rx=3, stroke="#2E2E2E")
    body += t(fx + 7, cy + 14.5, "Selection", 12.5) + icon("dropdown", fx + fw - 20, cy + 3); cy += 24
    body += toggle_row(x, fx, cy, "Use Renderer Bounds", True); cy += 24 + 10

    colors = ["#ED615A", "#99D959", "#6199F2"]
    bw = (w - 16 - 22 - 12) / 4
    for axis, name in enumerate("XYZ"):
        body += t(x + 12, cy + 16, name, 13.5, colors[axis], weight=700)
        for j, label in enumerate(["Min", "Center", "Max", "Distribute"]):
            body += mini_button(x + 30 + j * (bw + 4), cy + 1, bw, 21, label, size=12.5)
        cy += 26
    cy += 6
    box, _ = helpbox(x + 6, cy, w - 12, ["4 objects selected."], ic=None)
    body += box

    # Before / after: aligning four boxes on Y > Min.
    sy = y + wh + 34
    body += r(30, sy, 460, 140, CARD, rx=10, stroke=CARD_BORDER)
    heights = [(34, 18), (22, 44), (40, 0), (28, 30)]
    base = sy + 104
    for k, (bh, lift) in enumerate(heights):
        body += r(58 + k * 40, base - bh - lift, 28, bh, ACCENT, rx=3, opacity=0.9)
    body += f'<path d="M232 {sy + 70} H268 M260 {sy + 62} L268 {sy + 70} L260 {sy + 78}" fill="none" stroke="{DESC}" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>'
    for k, (bh, _) in enumerate(heights):
        body += r(300 + k * 40, base - bh, 28, bh, ACCENT, rx=3, opacity=0.9)
    body += f'<path d="M290 {base} H470" stroke="#E5534B" stroke-width="1.6" stroke-dasharray="5 4"/>'
    body += t(122, sy + 128, "Before", 12.5, DESC, anchor="middle")
    body += t(384, sy + 128, "Y › Min", 12.5, DESC, anchor="middle")
    return svg(W, H, body)


FILES = {
    "banner.svg": banner,
    "hierarchy.svg": hierarchy,
    "scene-switcher.svg": scene_switcher,
    "missing-scripts.svg": missing_scripts,
    "batch-renamer.svg": batch_renamer,
    "shortcuts.svg": shortcuts,
    "runtime.svg": runtime,
    "prefab-replacer.svg": prefab_replacer,
    "favorites.svg": favorites,
    "find-references.svg": find_references,
    "align-distribute.svg": align_distribute,
}

for filename, make in FILES.items():
    with open(os.path.join(OUT, filename), "w", encoding="utf-8", newline="\n") as f:
        f.write(make())
    print("wrote", filename)
