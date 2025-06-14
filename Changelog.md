# 5.5.1.0

- Add search

# 5.5.0.0

- Add support for Protector's Focus psycast

# 5.4.0.1

- Attempted fix at possible NRE in Pawn_Tick_Patch

# 5.4.0.0

- Fix Word of Immunity not checking for immunizable hediffs

# 5.3.0.0

- Expose Debug Logging option for reporting issues

# 5.2.1.0

- Actually use LowJoy in Word of Joy handling

# 5.2.0.0

- Update the development environment
- Fix LowJoy helper method using mood value instead of mood percentage when choosing targets

# 5.1.0.0

- Add support for Etch Runecircle from VPE Runesmith
- Add options for hiding unwanted area designators
  - with 5 different Areas, it was starting to get crowded
- Move more files into proper Compatibility folders
- Small fixes in settings GUI

# 5.0.3.0

- Add minimum psyfocus threshold (Thanks to REDIZIT on GitHub)

# 5.0.2.3

- Use LoadFolders.xml instead of PatchOperationFindMod

# 5.0.2.2

- Fix "Colony Animals" checkbox for Deathshield setting "Colonist" setting instead

# 5.0.2.1

- Remove line of sight requirement from Ice Crystal

# 5.0.2

Start keeping a proper changelog

- Make Solar Pinhole no longer require line of sight
  - also applies for the variant from VPE Solar Pinhole Sunlamp
- Add an option to show target validation failure causes as messages
- Fix Mend trying to autocast on untargetable objects (e.g. plants in stockpiles)
- Properly compile as Release build (5.0.1 appears to have been a debug build)
