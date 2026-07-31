# Dark Theme System

**Project:** Personal Business Manager  
**Decision:** P1-05 — Complete the dark-theme design specification  
**Document status:** Approved Phase 1 design baseline  
**Date:** 29 July 2026  
**Owner:** Charlie Cook  
**Target platform:** C# WinForms, .NET 10  
**Repository path:** `docs/design/dark_theme_system.md`  
**Related documents:** `personal_business_management_application_final_plan.md`, `docs/wireframes/`, `workflow_codes.md`

---

## 1. Purpose

This document is the source of truth for the Personal Business Manager visual design system.

It defines the shared:

- colour tokens;
- typography;
- spacing;
- control dimensions;
- page layout dimensions;
- grid dimensions;
- borders and corner treatment;
- focus, hover, pressed and disabled states;
- success, warning, error and information treatment;
- loading, empty and validation states;
- DPI-scaling behaviour;
- minimum window behaviour;
- reusable WinForms theme infrastructure.

No form, page, dialog or reusable control should invent its own colour, font size, spacing value or interaction-state styling unless this document is deliberately updated.

Dark mode is the required MVP appearance. Light mode, user-selectable accent colours and custom dashboard styling are outside the approved first-version scope.

---

# 2. Design principles

## 2.1 Consistency before decoration

The application should look deliberately designed, but visual consistency is more important than decorative effects.

Use:

- a restrained dark surface hierarchy;
- one purple accent family;
- clear typography;
- predictable spacing;
- subtle borders;
- limited shadows;
- consistent control heights;
- explicit loading, empty, error and validation states.

Avoid:

- gradients;
- glass effects;
- strong drop shadows;
- excessive rounded corners;
- bright colours covering large areas;
- decorative animations;
- per-page colour schemes;
- unthemed standard WinForms controls mixed with themed controls.

## 2.2 Information hierarchy

A user should be able to distinguish, in order:

1. the current page;
2. the primary action;
3. the main data or form content;
4. warnings and validation;
5. secondary metadata;
6. disabled or unavailable controls.

Use typography, spacing and surface elevation before adding more colour.

## 2.3 Dark from the beginning

Do not build light standard WinForms forms and plan to theme them later.

Every new form, page and control must use the shared theme infrastructure when it is first implemented.

## 2.4 Accessibility

The design must:

- remain readable at common Windows scaling levels;
- provide visible keyboard focus;
- never communicate a status using colour alone;
- pair semantic colour with text and, where useful, an icon;
- preserve readable contrast between text and its background;
- provide sufficiently large click and focus targets;
- keep disabled information readable;
- support keyboard navigation and logical tab order;
- avoid flashing or decorative movement.

## 2.5 Desktop-first density

This is an information-heavy desktop application.

The design should be compact enough for lists, finance records and administrative workflows, but not cramped.

The default density is:

```text
Comfortable desktop
```

A future density setting is not included in the MVP.

---

# 3. Colour system

## 3.1 Colour-token rules

1. All colours are accessed through `ThemePalette`.
2. Forms and controls must not contain literal hex or RGB values.
3. Semantic colours are not used as large page backgrounds.
4. Accent colour indicates selection and primary action, not every clickable item.
5. Text over a filled accent, success or warning button uses dark text where required for readability.
6. Muted text is never used for essential instructions, values or validation.
7. Archived, completed and disabled states still include readable text labels.
8. Transparency should be avoided in ordinary WinForms controls because compositing support is inconsistent. Use explicit colours instead.

## 3.2 Core surface tokens

| Token | Hex | Intended use |
|---|---:|---|
| `ApplicationBackground` | `#111318` | Main application canvas behind the shell and pages. |
| `SidebarBackground` | `#171A20` | Expanded and collapsed sidebar. |
| `HeaderBackground` | `#171A20` | Main shell top header. |
| `PanelBackground` | `#1D2128` | Standard cards, sections and page panels. |
| `RaisedPanel` | `#242932` | Summary cards, dialogs, menus and raised content. |
| `InputBackground` | `#191D23` | Text boxes, combo boxes and date inputs. |
| `InputHoverBackground` | `#20252D` | Hovered editable controls. |
| `InputDisabledBackground` | `#20242B` | Disabled and read-only inputs. |
| `OverlayBackground` | `#0B0D11` | Loading/modal overlay base; apply controlled opacity in custom painting only. |
| `TooltipBackground` | `#2A303A` | Tooltips and small popovers. |
| `GridAlternateRow` | `#20252C` | Alternating DataGridView rows. |
| `GridSelectedRow` | `#302B55` | Selected DataGridView row. |
| `GridHoverRow` | `#282D37` | Optional hovered DataGridView row. |

Surface hierarchy:

```text
ApplicationBackground
    └── PanelBackground
          └── RaisedPanel
```

Do not place several raised surfaces inside one another without a real hierarchy reason.

## 3.3 Text tokens

| Token | Hex | Intended use |
|---|---:|---|
| `PrimaryText` | `#F1F3F5` | Headings, labels, primary values and body text. |
| `SecondaryText` | `#AAB1BB` | Supporting descriptions and secondary values. |
| `MutedText` | `#8B94A3` | Timestamps, captions and low-priority metadata. |
| `DisabledText` | `#7F8896` | Disabled labels and control text. |
| `InverseText` | `#111318` | Text on accent, success and warning filled surfaces. |
| `LinkText` | `#A99FFF` | Inline textual links. |
| `LinkHoverText` | `#C1BAFF` | Hovered inline links. |
| `PlaceholderText` | `#7F8896` | Placeholder and hint text inside inputs. |

The original plan proposed `#747D89` for muted text. This specification uses `#8B94A3` because it remains more legible on `PanelBackground` and `RaisedPanel`.

Muted and disabled text are not interchangeable:

- `MutedText` is still meaningful information.
- `DisabledText` indicates an unavailable interaction.

## 3.4 Border and separator tokens

| Token | Hex | Width | Intended use |
|---|---:|---:|---|
| `BorderSubtle` | `#2B313B` | 1 px | Dividers and quiet card boundaries. |
| `BorderDefault` | `#343B46` | 1 px | Inputs, grids, panels and dialogs. |
| `BorderStrong` | `#505968` | 1 px | Hovered controls and stronger separation. |
| `FocusBorder` | `#A99FFF` | 2 px | Keyboard focus ring. |
| `Divider` | `#2B313B` | 1 px | Horizontal and vertical separators. |
| `SelectionIndicator` | `#7C6CF2` | 3 px | Selected sidebar item or active section bar. |

One-pixel values are logical pixels and are scaled deliberately where required. At high DPI, custom-drawn borders must remain visually clear and must not disappear because of integer rounding.

## 3.5 Accent tokens

| Token | Hex | Intended use |
|---|---:|---|
| `Accent` | `#7C6CF2` | Primary buttons, selected navigation and active controls. |
| `AccentHover` | `#9184F7` | Hovered primary actions. |
| `AccentPressed` | `#6959DC` | Pressed primary actions. |
| `AccentSoft` | `#302B55` | Selected rows, selected tabs and low-emphasis accent backgrounds. |
| `AccentBorder` | `#8F83F5` | Accent outlines and selected control borders. |
| `AccentText` | `#A99FFF` | Inline accent text and links. |

Filled accent controls use `InverseText`.

Do not use `PrimaryText` or white text over `Accent` merely by habit; dark `InverseText` is the approved filled-button treatment.

## 3.6 Semantic tokens

| Semantic role | Main | Soft background | Border | Text/icon |
|---|---:|---:|---:|---:|
| Success | `#46B981` | `#18352B` | `#2F8F68` | `#75D5A8` |
| Warning | `#D6A64A` | `#382D18` | `#A77D2E` | `#E6C16F` |
| Danger | `#DC5C68` | `#3B2026` | `#B84A56` | `#F0848E` |
| Information | `#5C9DED` | `#192C43` | `#3E78B9` | `#82B7F5` |
| Neutral | `#8B94A3` | `#292E37` | `#505968` | `#C1C7D0` |

Use semantic tokens for:

- status badges;
- validation summaries;
- notifications;
- warning banners;
- backup health;
- connection state;
- overdue indicators;
- destructive confirmations.

Semantic background panels use `PrimaryText` for their main message and the semantic text/icon token for accents.

## 3.7 Status mapping guidance

Status badges always display text.

Suggested mappings:

| Domain status | Semantic treatment |
|---|---|
| Planned / Not started / Draft | Neutral |
| Active / In progress / Open | Information or Accent |
| Completed / Paid / Backup successful | Success |
| On hold / Blocked / Part paid / Awaiting information | Warning |
| Cancelled / Declined / Failed / Overdue | Danger |
| Archived / Hidden / Closed | Neutral |
| Sent / Approved | Information |
| Credited / Reversed | Neutral with clear text |

The exact semantic mapping may vary by domain meaning, but a status must not change meaning solely because a colour was reused.

## 3.8 Colour-use restrictions

Do not:

- use red for ordinary delete icons that are not destructive;
- use green for all positive financial values;
- show liabilities only in red;
- show personal and business scope using colour alone;
- use muted text for form labels;
- display critical warnings only as a coloured border;
- create local colours such as `CustomerPurple` or `InvoiceBlue`.

---

# 4. Typography

## 4.1 Font family

Preferred:

```text
Segoe UI Variable
```

Fallback:

```text
Segoe UI
```

Do not bundle or distribute font files.

Use the installed Windows font through normal system APIs.

## 4.2 Type scale

WinForms font sizes are defined in points.

| Token | Size | Weight | Typical use |
|---|---:|---|---|
| `Caption` | 8.5 pt | Regular | Very small supporting text, never essential instructions. |
| `Small` | 9 pt | Regular | Grid metadata, badges and timestamps. |
| `Body` | 10 pt | Regular | Standard body text and input text. |
| `BodyStrong` | 10 pt | Semibold | Emphasised values and selected rows. |
| `Label` | 9.5 pt | Semibold | Form labels and compact section labels. |
| `Button` | 9.5 pt | Semibold | Buttons and navigation items. |
| `SectionHeading` | 12 pt | Semibold | Card and form section headings. |
| `DialogHeading` | 15 pt | Semibold | Dialog title inside the content area where needed. |
| `PageHeading` | 20 pt | Semibold | Main page title. |
| `DashboardValue` | 18 pt | Semibold | Summary-card primary numbers. |
| `MonospaceSmall` | 9 pt | Regular | Correlation IDs, hashes and technical safe identifiers. |

Recommended monospace fallback:

```text
Cascadia Mono
Consolas
```

Use monospace only for technical values, not general body copy.

## 4.3 Font-style rules

- Page and section headings use semibold rather than bold where available.
- Avoid all-uppercase body labels.
- Sidebar section captions may use uppercase at `Small` size with increased spacing.
- Do not underline text except links.
- Do not italicise important validation or financial values.
- Amounts use tabular or consistently aligned digits where practical.
- Negative values use a minus sign and text label where context requires; colour is secondary.
- Date display follows `en-GB`, such as `29/07/2026`.
- Currency display uses GBP formatting, such as `£1,250.00`.

## 4.4 Line and text spacing

WinForms does not expose CSS-style line height consistently.

Use these layout equivalents:

- body labels: minimum control height 24 px;
- wrapped body text: allow approximately 1.4 times the font height;
- paragraph separation: 8 px;
- label-to-input separation: 4 px;
- heading-to-content separation: 8 px;
- section-heading-to-previous-section separation: 24 px.

Do not vertically centre multi-line text inside an undersized control.

## 4.5 Text hierarchy rules

Each page should normally contain:

- one `PageHeading`;
- optional breadcrumb text at `Small` or `Body`;
- section headings at `SectionHeading`;
- labels at `Label`;
- content at `Body`;
- supporting metadata at `Small` or `MutedText`.

Avoid using font size alone to communicate hierarchy. Combine size with spacing and weight.

---

# 5. Spacing system

## 5.1 Approved spacing tokens

| Token | Value | Use |
|---|---:|---|
| `Space4` | 4 px | Label-to-input gap, icon-to-text gap, very tight grouping. |
| `Space8` | 8 px | Related controls, button groups, paragraph separation. |
| `Space16` | 16 px | Standard section and card internal spacing. |
| `Space24` | 24 px | Page padding and major section spacing. |
| `Space32` | 32 px | Large separation between distinct page regions. |

Do not introduce arbitrary values such as 13 px, 19 px or 27 px.

## 5.2 Allowed structural half-values

The following are permitted only for fixed visual mechanics, not general layout spacing:

| Value | Purpose |
|---:|---|
| 1 px | Standard border/divider. |
| 2 px | Focus border. |
| 3 px | Sidebar selection indicator. |
| 6 px | Standard corner radius where custom drawing is used. |
| 12 px | Compact badge horizontal padding or icon box. |

General form layout must still use the five approved spacing tokens.

## 5.3 Page padding

Default authenticated page padding:

```text
24 px top
24 px right
24 px bottom
24 px left
```

Compact shell layout below the responsive threshold:

```text
16 px horizontal
24 px vertical
```

Dialogs:

```text
24 px outer padding
16 px section spacing
```

Do not place controls directly against the shell content panel boundary.

## 5.4 Card padding and gaps

| Component | Internal padding | Gap between children |
|---|---:|---:|
| Summary card | 16 px | 8 px |
| Standard panel/card | 16 px | 16 px |
| Dense filter panel | 16 px | 8 px |
| Status banner | 16 px | 8 px |
| Empty-state panel | 24 px | 16 px |
| Dialog content | 24 px | 16 px |
| Dialog action bar | 16 px vertical, 24 px horizontal | 8 px |

## 5.5 Form layout

Standard vertical form rhythm:

```text
Label
4 px
Input
16 px
Next label
```

Related side-by-side controls:

```text
8 px gap
```

Distinct form sections:

```text
24 px gap
```

A form should use a `TableLayoutPanel`, `FlowLayoutPanel` or another layout container rather than manual pixel positioning for every field.

---

# 6. Shell and page dimensions

## 6.1 Main shell

| Element | Expanded size | Compact size |
|---|---:|---:|
| Sidebar width | 224 px | 64 px |
| Top header height | 64 px | 64 px |
| Timer strip height | 48 px | 48 px |
| Sidebar navigation item height | 40 px | 40 px |
| Sidebar section gap | 16 px | 16 px |
| Sidebar inner horizontal padding | 16 px | 8 px |
| Content page padding | 24 px | 16 px horizontal |
| Header action gap | 8 px | 8 px |

The timer strip is not allocated when no timer is active.

## 6.2 Responsive threshold

At a logical client width below:

```text
1180 px
```

the shell should:

- collapse the sidebar automatically;
- use compact horizontal page padding;
- move secondary header actions into an overflow menu;
- retain primary page actions;
- avoid horizontal scrolling for the entire page.

Individual wide grids may use their own horizontal scrolling only where column reduction cannot preserve meaning.

## 6.3 Page header

Standard `PageHeader`:

| Element | Value |
|---|---:|
| Minimum header content height | 64 px |
| Heading-to-subtitle gap | 4 px |
| Header-to-page-content gap | 24 px |
| Action-button gap | 8 px |
| Breadcrumb bottom gap | 8 px |

The page heading and the primary action remain visible at the minimum supported size.

## 6.4 Minimum supported window

At 100% Windows scaling:

```text
Minimum outer window size: 1100 × 700 px
Recommended working size: 1440 × 900 px
```

The application may start maximised or restore the user’s previous valid size.

At the minimum size:

- the sidebar is collapsed;
- the page uses compact padding;
- the timer strip remains usable;
- primary actions remain visible;
- forms remain scrollable inside the content region;
- the shell itself should not require horizontal scrolling.

Do not set a fixed maximum size.

---

# 7. Standard control dimensions

All measurements are logical pixels at 96 DPI.

## 7.1 Buttons

| Button type | Height | Minimum width | Horizontal padding |
|---|---:|---:|---:|
| Compact icon button | 32 px | 32 px | 8 px |
| Standard button | 36 px | 88 px | 16 px |
| Large/primary setup button | 44 px | 120 px | 24 px |
| Dialog action button | 36 px | 96 px | 16 px |
| Sidebar navigation item | 40 px | Container width | 16 px |
| Inline grid action | 32 px | 32 px or text fit | 8 px |

Button groups use an 8 px gap.

Do not make important actions smaller than 32 × 32 logical pixels.

## 7.2 Inputs

| Input type | Height/minimum |
|---|---:|
| Single-line text box | 36 px |
| Combo box | 36 px |
| Date/time picker | 36 px |
| Currency input | 36 px |
| Duration input | 36 px |
| Numeric input | 36 px |
| Search box | 36 px |
| Multi-line notes | 96 px minimum |
| Long description | 128 px minimum |
| Checkbox row | 32 px minimum |
| Radio option row | 32 px minimum |

Input internal horizontal padding:

```text
12 px
```

Icon-to-text gap:

```text
8 px
```

## 7.3 Tabs and filters

| Component | Value |
|---|---:|
| Tab header height | 40 px |
| Tab horizontal padding | 16 px |
| FilterBar minimum height | 60 px |
| Filter control gap | 8 px |
| Filter row vertical padding | 12 px |
| Paging footer height | 48 px |

A filter bar may wrap to a second row before the whole page scrolls horizontally.

## 7.4 Cards and panels

| Component | Minimum height |
|---|---:|
| Summary card | 112 px |
| Small status card | 80 px |
| Standard content card | Content-driven |
| Empty-state panel | 200 px |
| Validation summary | 64 px |
| Notification toast | 56 px |

Cards should use consistent widths within the same row.

## 7.5 Dialog sizes

| Dialog type | Recommended client width |
|---|---:|
| Confirmation | 440 px |
| Small form | 520 px |
| Standard edit form | 640 px |
| Large record form | 760 px |
| Invoice/time selection | 900 px |

Dialog height is content-driven up to the available work area.

If content exceeds the work area:

- keep the title and action bar fixed;
- scroll the central content region;
- never place the primary action below an unreachable area.

---

# 8. DataGridView specification

## 8.1 Dimensions

| Element | Value |
|---|---:|
| Column-header height | 40 px |
| Standard row height | 36 px |
| Comfortable/wrapped row height | 48 px |
| Cell horizontal padding | 12 px |
| Cell vertical padding | 8 px |
| Grid border | 1 px |
| Row divider | 1 px |
| Selection indicator | Whole row plus visible focus cue |

Use the standard 36 px row height for most lists.

Use 48 px only where rows legitimately require a second line.

## 8.2 Colours

| Grid part | Token |
|---|---|
| Grid background | `PanelBackground` |
| Header background | `RaisedPanel` |
| Header text | `SecondaryText` or `PrimaryText` |
| Standard row | `PanelBackground` |
| Alternate row | `GridAlternateRow` |
| Hover row | `GridHoverRow` |
| Selected row | `GridSelectedRow` |
| Grid line | `BorderSubtle` |
| Primary cell text | `PrimaryText` |
| Secondary cell text | `SecondaryText` |

## 8.3 Behaviour

- Enable double buffering.
- Use full-row selection for record lists.
- Show a clear keyboard focus rectangle on the active row/cell.
- Retain selected-row text readability.
- Use deterministic sorting.
- Display friendly empty, loading and error states outside or over the grid.
- Place paging below the grid.
- Do not load unlimited records.
- Align currency and numeric values to the right.
- Align dates consistently.
- Keep status text visible; badges may supplement it.
- Avoid excessive icon-only columns.
- Do not use bright gridlines.

## 8.4 Column widths

Use:

- fixed widths for short status/date/action columns;
- fill weight for names, titles and descriptions;
- minimum widths so headers remain understandable;
- ellipsis plus tooltip for clipped non-sensitive text.

Do not allow one description column to force the whole form beyond the minimum window width.

---

# 9. Border, radius and elevation

## 9.1 Border widths

| Purpose | Width |
|---|---:|
| Standard input/panel border | 1 px |
| Hover border | 1 px |
| Keyboard focus border | 2 px |
| Selected navigation indicator | 3 px |
| Validation emphasis | 2 px |
| Dialog boundary | 1 px |

## 9.2 Corner radius

Where custom-drawn rounded corners are practical:

| Component | Radius |
|---|---:|
| Buttons | 6 px |
| Inputs | 6 px |
| Cards | 6 px |
| Status badges | 6 px |
| Dialog panels | 8 px |
| Tooltips | 6 px |

WinForms does not require every control to be rounded.

A consistently square 1 px border is preferable to an unreliable or poorly clipped rounded control.

Do not create pill-shaped ordinary buttons.

## 9.3 Elevation and shadows

Use shadows sparingly for:

- modal dialogs;
- floating menus;
- notification toasts.

Standard cards and page panels use surface contrast and borders instead of shadows.

If a custom shadow is used:

- keep it soft;
- use low opacity;
- do not extend more than 8 px;
- ensure it does not interfere with DPI scaling or redraw performance.

---

# 10. Interaction states

## 10.1 Focus

Keyboard focus must always be visible.

Approved focus treatment:

```text
2 px FocusBorder
plus an existing 1 px control border where applicable
```

Rules:

- focus must not rely only on a subtle background change;
- focused buttons show a 2 px outer or inset accent border;
- focused inputs use `FocusBorder`;
- focused grid rows/cells show both selected background and a visible outline;
- focus treatment must remain visible against accent and semantic surfaces;
- mouse interaction does not permanently suppress keyboard focus cues;
- tab order follows the visual reading order.

For native controls that do not permit adequate focus styling, create a themed wrapper or owner-drawn control.

## 10.2 Hover

### Primary button

```text
Background: AccentHover
Text: InverseText
Border: AccentHover
```

### Secondary button

```text
Background: RaisedPanel
Border: BorderStrong
Text: PrimaryText
```

### Ghost button

```text
Background: InputHoverBackground
Text: PrimaryText
```

### Input

```text
Background: InputHoverBackground
Border: BorderStrong
```

### Sidebar item

```text
Background: InputHoverBackground
Text: PrimaryText
```

Hover must not move or resize a control.

## 10.3 Pressed

Pressed state:

- uses `AccentPressed` for primary actions;
- uses a slightly darker explicit surface for secondary/ghost actions;
- may shift icon/content by at most one logical pixel if custom-drawn;
- must not remove the focus cue;
- lasts only while pressed.

## 10.4 Disabled

Disabled controls use:

```text
Background: InputDisabledBackground
Border: BorderSubtle
Text: DisabledText
```

Rules:

- do not use opacity alone;
- retain readable labels and current values;
- disabled form fields should explain why through nearby help text or tooltip where the reason is not obvious;
- disabled buttons do not respond to hover;
- disabled controls are skipped appropriately in keyboard tab order;
- read-only and disabled are visually distinct.

Read-only input:

```text
Background: PanelBackground
Border: BorderDefault
Text: SecondaryText or PrimaryText
```

A read-only value remains selectable/copyable where safe.

## 10.5 Selected

Selected navigation/tab/list state uses:

- `AccentSoft` background;
- `PrimaryText`;
- a 3 px `SelectionIndicator` where practical;
- semibold text only where it does not cause layout shift.

Selection remains distinguishable from keyboard focus.

## 10.6 Destructive actions

Ordinary destructive actions should normally be secondary/outlined until the final confirmation.

Final destructive confirmation may use:

```text
Background: Danger
Text: InverseText
Border: Danger
```

Use clear verbs:

```text
Archive customer
Reverse payment
Restore backup
Discard changes
```

Avoid generic labels such as `Yes` for consequential actions.

---

# 11. Button hierarchy

## 11.1 Primary

Use for the single most important action in a page or dialog.

Examples:

- Save customer.
- Finalise invoice.
- Start timer.
- Record payment.
- Create backup.

Style:

```text
Accent background
InverseText
Accent border
```

A page should normally have no more than one prominent primary button in the same action group.

## 11.2 Secondary

Use for meaningful alternatives.

Examples:

- Save draft.
- Export CSV.
- Update balance.
- Edit.

Style:

```text
RaisedPanel background
PrimaryText
BorderDefault
```

## 11.3 Ghost

Use for low-emphasis actions.

Examples:

- Clear filters.
- Cancel.
- Open related record.
- Overflow menu.

Style:

```text
Transparent or parent background
SecondaryText
No border until hover, unless focus requires it
```

## 11.4 Danger

Use only where the action is destructive or reversing.

The danger colour must not be used for ordinary archive navigation icons unless the current action is genuinely destructive.

---

# 12. Input and form styling

## 12.1 Labels

- Label text uses `PrimaryText`.
- Required fields append a text-visible asterisk.
- Optional fields may use `(optional)` in `MutedText`.
- Labels remain above inputs by default.
- Side-by-side label/input forms are reserved for read-only summaries or dense settings.

## 12.2 Text boxes

Normal:

```text
InputBackground
BorderDefault
PrimaryText
PlaceholderText
```

Focused:

```text
FocusBorder
```

Invalid:

```text
Danger border at 2 px
ValidationMessage below
```

Do not clear invalid text automatically.

## 12.3 Combo boxes

- Use the same height and border treatment as text boxes.
- The dropdown arrow remains visible at all DPI levels.
- The dropdown list uses `RaisedPanel`, `PrimaryText` and `AccentSoft` selection.
- Searchable selectors use a dedicated search/picker dialog where the list is large.
- Do not load hundreds of database records into a normal combo box.

## 12.4 Date/time pickers

- Use `en-GB` display.
- Date-only fields display `dd/MM/yyyy`.
- UTC event times are converted for display through the application clock/time-zone service.
- Nullable dates require an explicit checkbox or clear action.
- Calendar popup contrast must be manually verified because native rendering may differ.

## 12.5 Checkboxes and radio buttons

- Minimum interaction row height: 32 px.
- Label text uses `PrimaryText`.
- Checked state uses `Accent`.
- Keyboard focus surrounds both indicator and label.
- Do not use a checkbox to trigger a dangerous action immediately without confirmation.

## 12.6 Currency and duration inputs

`CurrencyTextBox`:

- displays GBP conventions;
- accepts decimal input;
- right-aligns values;
- does not use `float` or `double`;
- distinguishes blank from zero;
- validates without silently changing an entered value.

`DurationTextBox`:

- supports the approved duration-entry pattern;
- displays clear hours/minutes labels;
- never converts exact stored seconds ambiguously;
- explains rounding separately from raw duration.

---

# 13. Tabs, navigation and breadcrumbs

## 13.1 Sidebar

Normal item:

```text
SidebarBackground
SecondaryText
```

Hovered:

```text
InputHoverBackground
PrimaryText
```

Selected:

```text
AccentSoft
PrimaryText
3 px SelectionIndicator
```

Sidebar section labels use:

```text
Small semibold
MutedText
uppercase permitted
```

Collapsed sidebar:

- retains 40 px item height;
- uses centred icons;
- shows the destination in a tooltip;
- keeps the selected indicator;
- remains keyboard accessible.

## 13.2 Tabs

Inactive:

```text
PanelBackground
SecondaryText
```

Hovered:

```text
InputHoverBackground
PrimaryText
```

Selected:

```text
RaisedPanel or AccentSoft
PrimaryText
2–3 px accent indicator
```

Tab selection must remain visible without relying on font weight alone.

## 13.3 Breadcrumbs

- Use `Small` or `Body`.
- Parent links use `LinkText`.
- Current item uses `SecondaryText`.
- Separate levels with a quiet chevron.
- Do not show an excessively deep breadcrumb; use the logical hierarchy from the approved wireframes.

---

# 14. Feedback components

## 14.1 Validation messages

Field-level validation:

```text
Danger icon
Danger semantic text
9 pt font
4 px below the field
```

Validation summary:

```text
DangerSoft background
Danger border
PrimaryText heading
Clear issue list
Link to first invalid field
```

Do not use message boxes for normal field validation.

## 14.2 Error state

Page/section error:

```text
Danger icon
Clear user-friendly heading
Short safe description
Correlation ID in monospace where available
Retry action
Safe navigation action
```

Do not display raw exception text or stack traces.

## 14.3 Warning state

Use `Warning` for:

- overdue timers;
- overdue invoices/tasks;
- failed backup health;
- unsaved changes;
- estimates requiring caution;
- irreversible next steps.

Warning does not always mean the action is blocked.

## 14.4 Success state

Use success notifications for completed operations:

- customer saved;
- invoice finalised;
- payment recorded;
- backup completed;
- account balance updated.

Do not rely on a success toast as the only evidence that a financial operation occurred; update the underlying page state.

## 14.5 Information state

Use information styling for:

- explanatory notices;
- scope labels;
- read-only system status;
- deferred/not-yet-configured guidance;
- estimate methodology links.

## 14.6 Notifications

Toast dimensions:

```text
Minimum height: 56 px
Maximum width: 420 px
Internal padding: 16 px
Stack gap: 8 px
```

Placement:

```text
Top-right of the shell content area
below the main header
above the timer strip
```

Success notifications may auto-dismiss.

Warnings and errors remain until dismissed or resolved.

## 14.7 Empty states

An empty state contains:

1. a clear heading;
2. one sentence explaining the state;
3. one primary next action where relevant;
4. an optional secondary clear-filter or show-archived action.

Do not show an unexplained blank grid.

## 14.8 Loading states

Use:

- `LoadingOverlay` for page or panel operations;
- skeleton rows/cards for initial dashboard/list loading where practical;
- indeterminate progress for unknown-duration operations;
- staged progress for backup and restore.

Rules:

- disable duplicate write actions;
- do not block unrelated shell navigation unnecessarily;
- keep the active timer visible and usable where safe;
- do not fake progress percentages;
- display a cancel action only when cancellation is genuinely supported.

---

# 15. Cards and badges

## 15.1 Summary cards

Structure:

```text
Small label
DashboardValue
Secondary supporting value
Optional icon
```

Style:

```text
RaisedPanel
BorderSubtle
16 px padding
6 px radius
```

Cards are clickable only when they have a real destination.

Clickable cards show hover and focus treatment.

## 15.2 Status badges

Badge specification:

```text
Minimum height: 24 px
Horizontal padding: 8–12 px
Small semibold text
6 px radius
Semantic soft background
Semantic border
Semantic text/icon
```

Badge text must display the status.

Do not use a small coloured circle without text in primary workflows.

---

# 16. Dialog and modal specification

## 16.1 Structure

```text
Title area
Content area
Validation summary where required
Action bar
```

Dialog background:

```text
RaisedPanel
```

Dialog border:

```text
BorderDefault
```

The action bar may use `PanelBackground` to create separation.

## 16.2 Behaviour

- Default action is explicit and safe.
- `Esc` closes only when no destructive operation is already in progress.
- `Enter` triggers the default action only when appropriate.
- Destructive confirmations use action-specific button text.
- A dialog cannot open behind the main form.
- Owner and focus are set correctly.
- Long-running modal operations remain responsive.
- Unsaved data is not discarded silently.

## 16.3 Confirmation levels

Simple confirmation:

- archive non-financial records;
- discard ordinary unsaved changes.

Strong confirmation:

- finalise an invoice;
- reverse a payment;
- replace data during restore;
- regenerate security recovery code.

Strong confirmation includes a clear consequence statement.

Backup restore may require typed confirmation.

---

# 17. Iconography

## 17.1 General rules

- Use one consistent icon family available to the application.
- Prefer simple outlined icons.
- Standard icon size: 16 px.
- Primary toolbar icon size: 20 px.
- Sidebar icon box: 20–24 px.
- Icons inherit the relevant text or semantic colour.
- Icons supplement labels; they do not replace labels for important actions.
- Provide tooltips for icon-only buttons.
- Use high-DPI/vector-capable resources or appropriately sized image assets.

## 17.2 Common icon meanings

Keep meanings consistent:

| Action | Icon concept |
|---|---|
| Add | Plus |
| Edit | Pencil |
| Archive | Archive box |
| Restore | Restore arrow |
| Search | Magnifier |
| Filter | Funnel |
| Clear | X/eraser with text |
| Save | Save/check |
| Export | Arrow out/document |
| Backup | Database/drive |
| Warning | Triangle |
| Error | Circle/X |
| Success | Circle/check |
| Information | Circle/i |
| Overflow | Horizontal or vertical ellipsis |

Do not use a bin icon for archive if the record is not being deleted.

---

# 18. DPI scaling

## 18.1 Baseline

Design measurements are logical pixels at:

```text
96 DPI / 100% scaling
```

All top-level forms use:

```csharp
AutoScaleMode = AutoScaleMode.Dpi;
```

Use one scaling strategy consistently. Do not mix automatic DPI scaling with manually multiplying every WinForms layout dimension a second time.

## 18.2 Required test levels

Manually verify:

```text
100%
125%
150%
```

Also perform a smoke test at:

```text
175%
200%
```

before production release.

## 18.3 Scaling rules

- Use docking, anchoring and layout panels.
- Avoid fixed absolute coordinates for whole forms.
- Use `AutoSize` deliberately for labels and wrapped messages.
- Give form fields sensible minimum and maximum widths.
- Prevent labels from clipping.
- Do not scale fonts manually if WinForms is already scaling them.
- Scale custom-drawn border, icon and radius values using the control’s device DPI.
- Use high-DPI image resources.
- Recalculate custom layout when `DpiChanged` occurs.
- Test moving the application between monitors with different scaling.
- Store and restore window bounds safely within the current monitor work area.
- Do not restore a previous size smaller than the approved minimum.

## 18.4 Logical scaling helper

Custom-drawn controls may use a central helper equivalent to:

```csharp
public static int Scale(int logicalPixels, int deviceDpi)
{
    return (int)Math.Round(logicalPixels * deviceDpi / 96d);
}
```

Use it only for custom-drawn or manually sized token values not already scaled by WinForms.

## 18.5 DPI failure conditions

A screen fails DPI verification if:

- text is clipped;
- buttons overlap;
- the primary action leaves the visible area;
- icons become blurry or too small;
- grid rows cannot display their text;
- focus borders disappear;
- dialogs exceed the monitor work area without a scrollable content region;
- sidebar or timer controls become unusable;
- the tab order no longer follows the visual layout.

---

# 19. Window and responsive behaviour

## 19.1 Main-window rules

- The application is resizable.
- The minimum outer size is 1100 × 700 at 100%.
- The sidebar collapses below the logical width threshold.
- Page content uses vertical scrolling where required.
- The timer strip remains fixed to the shell bottom when active.
- The header remains fixed.
- Grids expand to available space.
- Dialogs are centred over the owner and constrained to the work area.

## 19.2 Compact behaviour

When horizontal space is limited:

1. collapse the sidebar;
2. reduce page horizontal padding from 24 to 16 px;
3. move secondary header actions into overflow;
4. allow filter bars to wrap;
5. hide only genuinely secondary grid columns;
6. retain primary identity, status and action columns;
7. use horizontal grid scrolling only as a final fallback.

## 19.3 No mobile layout

A mobile or touch-first layout is not required for the WinForms MVP.

Controls must still be large enough for ordinary pointer and accessibility use.

---

# 20. Reusable control specification

## 20.1 Required theme infrastructure

```text
Theming/
├── ThemePalette.cs
├── UiSpacing.cs
├── UiFonts.cs
├── UiDimensions.cs
├── ThemeManager.cs
├── ControlStyler.cs
└── DpiScaler.cs
```

`ThemePalette` already exists and should be improved rather than duplicated.

## 20.2 Required themed controls

```text
DarkButton
DarkTextBox
DarkComboBox
DarkDateTimePicker
DarkDataGridView
DarkTabControl
PageHeader
FilterBar
SummaryCard
StatusBadge
EmptyStatePanel
LoadingOverlay
ValidationMessage
ConfirmDialog
```

Deferred until first functional use:

```text
CurrencyTextBox
DurationTextBox
```

They must still follow this specification when implemented.

## 20.3 ThemeManager responsibilities

`ThemeManager` should:

- apply form and page background colours;
- style known standard controls;
- apply fonts;
- apply colours to container children where safe;
- avoid repeatedly attaching event handlers;
- support designer-created controls;
- avoid overriding a control’s explicit semantic style;
- apply theme after dynamic controls are created;
- provide a development-only validation method to detect unthemed controls.

## 20.4 ControlStyler responsibilities

Use focused methods rather than one uncontrolled recursive method:

```text
StyleForm
StylePanel
StyleLabel
StyleButton
StyleInput
StyleDataGridView
StyleTabControl
StyleToolStrip
StyleContextMenu
StyleDialog
```

Do not identify important styling solely from control names.

Prefer typed themed controls or explicit semantic variants.

## 20.5 Button variants

Suggested enum:

```csharp
public enum ButtonVariant
{
    Primary,
    Secondary,
    Ghost,
    Danger
}
```

Suggested size enum:

```csharp
public enum ControlSize
{
    Compact,
    Standard,
    Large
}
```

These enums are UI-only and are not database workflow values.

## 20.6 StatusBadge model

A badge should accept:

```text
Display text
Semantic role
Optional icon
Accessible description
```

Do not pass raw domain codes directly to a drawing method without a mapping layer.

---

# 21. Suggested C# token shape

The exact implementation may vary, but it should provide one central source.

```csharp
public static class ThemePalette
{
    public static readonly Color ApplicationBackground = ColorTranslator.FromHtml("#111318");
    public static readonly Color SidebarBackground = ColorTranslator.FromHtml("#171A20");
    public static readonly Color HeaderBackground = ColorTranslator.FromHtml("#171A20");
    public static readonly Color PanelBackground = ColorTranslator.FromHtml("#1D2128");
    public static readonly Color RaisedPanel = ColorTranslator.FromHtml("#242932");
    public static readonly Color InputBackground = ColorTranslator.FromHtml("#191D23");
    public static readonly Color InputHoverBackground = ColorTranslator.FromHtml("#20252D");
    public static readonly Color InputDisabledBackground = ColorTranslator.FromHtml("#20242B");

    public static readonly Color PrimaryText = ColorTranslator.FromHtml("#F1F3F5");
    public static readonly Color SecondaryText = ColorTranslator.FromHtml("#AAB1BB");
    public static readonly Color MutedText = ColorTranslator.FromHtml("#8B94A3");
    public static readonly Color DisabledText = ColorTranslator.FromHtml("#7F8896");
    public static readonly Color InverseText = ColorTranslator.FromHtml("#111318");

    public static readonly Color BorderSubtle = ColorTranslator.FromHtml("#2B313B");
    public static readonly Color BorderDefault = ColorTranslator.FromHtml("#343B46");
    public static readonly Color BorderStrong = ColorTranslator.FromHtml("#505968");
    public static readonly Color FocusBorder = ColorTranslator.FromHtml("#A99FFF");

    public static readonly Color Accent = ColorTranslator.FromHtml("#7C6CF2");
    public static readonly Color AccentHover = ColorTranslator.FromHtml("#9184F7");
    public static readonly Color AccentPressed = ColorTranslator.FromHtml("#6959DC");
    public static readonly Color AccentSoft = ColorTranslator.FromHtml("#302B55");

    public static readonly Color Success = ColorTranslator.FromHtml("#46B981");
    public static readonly Color Warning = ColorTranslator.FromHtml("#D6A64A");
    public static readonly Color Danger = ColorTranslator.FromHtml("#DC5C68");
    public static readonly Color Information = ColorTranslator.FromHtml("#5C9DED");
}
```

Spacing:

```csharp
public static class UiSpacing
{
    public const int Space4 = 4;
    public const int Space8 = 8;
    public const int Space16 = 16;
    public const int Space24 = 24;
    public const int Space32 = 32;
}
```

Dimensions:

```csharp
public static class UiDimensions
{
    public const int StandardControlHeight = 36;
    public const int CompactControlHeight = 32;
    public const int LargeControlHeight = 44;
    public const int StandardButtonMinimumWidth = 88;
    public const int LargeButtonMinimumWidth = 120;
    public const int SidebarNavigationHeight = 40;
    public const int TabHeaderHeight = 40;
    public const int TabHeaderWidth = 128;
    public const int GridHeaderHeight = 40;
    public const int GridRowHeight = 36;
    public const int ComfortableGridRowHeight = 48;
    public const int GridCellHorizontalPadding = 12;
    public const int ExpandedSidebarWidth = 224;
    public const int CollapsedSidebarWidth = 64;
    public const int HeaderHeight = 64;
    public const int TimerStripHeight = 48;
    public const int SummaryCardHeight = 112;
    public const int SummaryCardWidth = 240;
    public const int MinimumWindowWidth = 1100;
    public const int MinimumWindowHeight = 700;
    public const int ResponsiveWidth = 1180;
    public const int StandardBorderWidth = 1;
    public const int FocusBorderWidth = 2;
    public const int SelectionIndicatorWidth = 3;
    public const int CornerRadius = 6;
}
```

Fonts should be created centrally and disposed appropriately with the application lifetime.

Do not create a new `Font` object during every paint event.

---

# 22. Native WinForms limitations

Some native controls do not fully respect `BackColor`, `ForeColor` or custom borders.

When a native control cannot meet the approved theme:

1. use owner drawing where stable;
2. wrap it in a themed control;
3. replace it with a small reviewed custom control;
4. document any unavoidable operating-system rendering exception.

Do not use broad Windows API hacks without testing disposal, DPI and accessibility.

Controls that require particular attention:

- `DateTimePicker`;
- `ComboBox` dropdown;
- `TabControl`;
- `DataGridView`;
- `ToolStrip` and `ContextMenuStrip`;
- scrollbars;
- native message boxes.

Use `ConfirmDialog` rather than the standard `MessageBox` for important branded confirmations.

A native file picker is an accepted operating-system dialog and does not need to match every application token.

---

# 23. Manual visual test matrix

Every reusable themed control must be checked in a development-only control gallery or equivalent manual test form.

## 23.1 Control states

For each control verify:

```text
Normal
Hovered
Focused by keyboard
Pressed/open
Selected
Read-only
Disabled
Validation error
Long text
High DPI
```

## 23.2 Scaling

Verify at:

```text
100%
125%
150%
```

Smoke-test:

```text
175%
200%
```

## 23.3 Window sizes

Verify:

```text
1100 × 700
1280 × 720
1440 × 900
1920 × 1080
```

Also test a maximised window and movement between monitors with different scaling.

## 23.4 Core screens

At minimum manually inspect:

- login;
- first-run administrator setup;
- main shell expanded and collapsed;
- dashboard;
- one list with grid/filter/paging;
- one detail page with tabs;
- one edit dialog;
- invoice editor;
- active timer and forgotten-timer warning;
- validation summary;
- empty state;
- loading overlay;
- notification;
- backup restore confirmation;
- settings.

## 23.5 Test failures

A theme implementation is not complete if:

- any control uses default light colours;
- text is clipped;
- focus is invisible;
- disabled values are unreadable;
- state is communicated only through colour;
- custom controls flicker noticeably;
- grid selection makes text unreadable;
- a standard dialog unexpectedly appears in a light theme for a workflow that should use `ConfirmDialog`;
- scaling creates overlap;
- spacing differs arbitrarily between equivalent screens.

---

# 24. Implementation rules for Codex

When implementing or changing UI:

1. Read this document and the relevant wireframe.
2. Reuse existing theme tokens and controls.
3. Do not add literal `Color.FromArgb` values inside forms.
4. Do not create a new font in a page.
5. Use the approved spacing constants.
6. Use the standard control heights.
7. Use layout panels, docking and anchoring.
8. Preserve visible keyboard focus.
9. Add normal, empty, loading, error and validation states.
10. Test at 100%, 125% and 150%.
11. Do not add light-theme branches.
12. Do not add accent-colour preferences.
13. Do not silently change the palette.
14. Update this document before introducing a new system-wide token.
15. Keep UI-specific enums and style variants out of database persistence.
16. Report any native WinForms control that cannot meet the specification.

---

# 25. P1-05 verification checklist

## Required content

- [x] Colour tokens are defined.
- [x] Typography sizes and weights are defined.
- [x] Spacing tokens use 4/8/16/24/32 px.
- [x] Standard control heights are defined.
- [x] Standard page padding is defined.
- [x] Grid row and header heights are defined.
- [x] Border widths are defined.
- [x] Focus styling is defined.
- [x] Disabled styling is defined.
- [x] Hover and pressed styling are defined.
- [x] Error styling is defined.
- [x] Warning styling is defined.
- [x] Success styling is defined.
- [x] Information and neutral styling are defined.
- [x] DPI-scaling expectations are defined.
- [x] Minimum supported window size is defined.
- [x] Sidebar, header and timer-strip dimensions are defined.
- [x] Dialog dimensions are defined.
- [x] Reusable control responsibilities are defined.
- [x] WinForms native-control limitations are documented.
- [x] Manual visual verification criteria are defined.

## Phase 2 implementation evidence still required

- [x] Update the existing `ThemePalette` to match the approved tokens.
- [x] Add `UiSpacing`.
- [x] Add `UiFonts`.
- [x] Add `UiDimensions`.
- [x] Add or complete `ThemeManager`.
- [x] Add or complete `ControlStyler`.
- [x] Add `DpiScaler` where custom drawing requires it.
- [ ] Complete the required themed controls.
- [ ] Add a development-only control gallery or equivalent visual test form.
- [x] Verify common screens at 100%, 125% and 150%.
- [x] Confirm no new screen contains hard-coded theme values.

P2-09 completed the shared infrastructure and verified the current main shell
and Dashboard at the required scale factors. The two remaining unchecked items
belong to P2-10 and keep the overall matching Phase 2 implementation decision
pending until the reusable control set and its gallery are complete.

---

# 26. Final decision

```text
Dark-theme palette:                  APPROVED
Typography system:                   APPROVED
Spacing system:                      APPROVED
Control dimensions:                  APPROVED
Page and shell dimensions:           APPROVED
Interaction states:                  APPROVED
DPI and minimum-window rules:        APPROVED
P1-05 documentation gate:            PASS
Matching Phase 2 C# implementation:  PENDING
```

No new screen should need to invent its own colour, font size, spacing value, standard control height, grid row height, border width or common interaction-state styling.

---

## 27. Approval record

**Owner:** Charlie Cook  
**Approval date:** 29 July 2026  
**Status:** Approved Phase 1 design baseline

The dark theme is mandatory for the first version.

The following remain outside MVP scope:

- light theme;
- user-selectable accent colours;
- custom dashboard themes;
- per-module colour schemes;
- selectable visual-density modes.
