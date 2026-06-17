# Space Checker – README

**Project Name**
Space Checker

**Revit Add-in Tab**
AACD Architect

**Panel**
General

---

## Description

Space Checker is a Revit add-in that checks a building's actual spaces against a
defined **space program**. It reads the **Areas** (apartments / units / commercial
spaces) and the **Rooms** inside them directly from the Revit model, compares each
one against required areas defined in an Excel sheet, and reports compliance with
clear, color-coded results.

It is built for **real residential and commercial layouts**: multiple apartments
per floor, identical stacked layouts, repeated instances of the same type, and
**duplexes that span more than one level**. Each apartment is an Area boundary;
the rooms inside it are matched to it **spatially** (point-in-polygon), so the
add-in works regardless of room naming.

---

## Features

* Reads **Areas** (apartments/units) and their **Rooms** straight from the model
* **Spatial room-to-area mapping** (point-in-polygon) — no manual tagging required
* Handles **residential complexity**: repeated instances per type, stacked
  identical layouts, and **duplexes spanning multiple levels**
* **Type-based space program**: define each space type once in Excel; every
  instance is checked against it
* **Create Template** — generates a ready-to-fill Excel program template
* **Import Program** — loads the filled-in Excel space program
* **Run Comparison** — actual vs. required, with an adjustable deviation tolerance
* **Color-coded master–detail results**: green = pass, amber = pass but rooms
  deviate (⚠), red = total area fails
* **Export Data** — dumps the model's areas + rooms to Excel in the same layout,
  so it can be reused or re-imported
* **Modeless window** owned by Revit — keep working in the model while it's open,
  and refresh the data on demand

---

## Setup Instructions

1. Run the setup file located in the **Release** folder.
2. The installer automatically places the `.addin` manifest and the required DLL
   files into the correct Revit Addins directories.
3. Supported Revit versions are installed simultaneously.
4. Launch Revit and locate the **“AACD Architect”** tab.
5. Open the **“General”** panel and click **“Space Checker”** to start the add-in.

---

## Compatibility

* Revit 2022
* Revit 2023
* Revit 2024

---

## Usage Instructions

1. Open a Revit project that has **Area boundaries** (one per apartment/space,
   each named) and **room tags** inside them.
2. Launch **Space Checker** from the **AACD Architect** tab. The add-in extracts
   the areas and rooms automatically on open.
3. Click **Create Template** to generate an Excel program, fill in the required
   areas per type, then **Import Program** — or import an existing program.
4. Set the **acceptable deviation (%)**.
5. Click **Run Comparison** to see each area's result, color-coded, with a
   per-room breakdown in the detail table.
6. *(Optional)* Edit the model and click **Extract Areas** to refresh, then
   re-run the comparison.
7. *(Optional)* Click **Export Data** to write the model's areas + rooms to Excel.

---

## Version

v1.0

---

## Contact

Mahmoud Allam
mahmoud.amr55@gmail.com
Developer & Designer

---

## Notes

* Pass/fail is decided by each area's **own total area** vs. the required total,
  within the deviation tolerance. Rooms that deviate inside a passing area are
  surfaced as a **warning (⚠)**, not a failure, so you know to check them.
* Comparison results are shown on screen only — they are not written back to Excel.
* Areas that match no program type (and unmapped rooms) are reported as
  **“Unassigned.”**
