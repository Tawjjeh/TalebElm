# TalebElm Brand & Media Assets

This directory contains visual brand assets for the TalebElm project repository.

## Files

| File | Type | Dimensions | Purpose |
|---|---|---|---|
| `banner-light.svg` | Vector (outline SVG) | 1800 x 520 | Light-theme banner with transparent background, teal accent rule, and tagline |
| `banner-dark.svg` | Vector (outline SVG) | 1800 x 520 | Dark-theme banner with transparent background, teal accent rule, and tagline |
| `banner-light.png` | Raster (PNG) | 1800 x 520 | Rendered light-theme banner composited on `#ffffff` |
| `banner-dark.png` | Raster (PNG) | 1800 x 520 | Rendered dark-theme banner composited on `#0d1117` |
| `banner-1800x480.svg` | Vector (outline SVG) | 1800 x 480 | Wide minimalist banner in pure Ruq'ah script |
| `banner-1800x480.png` | Raster (PNG) | 1800 x 480 | Rendered wide minimalist banner (aspect ratio 3.75:1) |
| `social-preview.svg` | Vector (SVG) | 1280 x 640 | Open Graph social card layout with calligraphy, title, and tagline |
| `social-preview.png` | Raster (PNG) | 1280 x 640 | GitHub Open Graph social preview (uploaded via Settings > General > Social preview) |

## Typography & Calligraphy Attribution

The Arabic calligraphy طالب علم (TalebElm) is rendered in the traditional Ruq'ah style using the open-source **Aref Ruqaa** font:

- **Font:** Aref Ruqaa Bold (`ArefRuqaa-Bold.ttf`)
- **Author:** Khaled Hosny and The Aref Ruqaa Project Authors (<https://github.com/alif-type/aref-ruqaa>)
- **License:** [SIL Open Font License, Version 1.1](https://scripts.sil.org/OFL) (OFL-1.1)

All SVG banners contain vectorized bezier path outlines (`<path d="..." />`) extracted using fontTools and HarfBuzz Arabic shaping. Client devices render the calligraphy accurately without needing the font installed.

The tagline uses system font stacks with generous margins. The Open Graph social preview is uploaded manually by repository owners through Settings > General > Social preview because GitHub does not expose an API endpoint for repository social preview images.
