# Radial
A text-based, scriptable, online RPG built with Blazor.

[![GitHub Actions](https://github.com/lucent-sea/Radial/actions/workflows/rsync-deploy.yml/badge.svg)](https://github.com/lucent-sea/Radial/actions/workflows/rsync-deploy.yml)

Website: https://radial.lucency.co

![Radial Screenshot](https://radial.lucency.co/media/screenshot.png "Radial Screenshot")

## Introduction

Note: This game is still in very early development.

Radial borrows a lot of concepts from MUDs, but I've tried to implement them in a way that are easily accessible on both mobile and desktop.  The core premise of the game is that your character has died, and you're now stuck in the center of a vast nothingness.

You will venture outward from the center, either alone or with a party, in search of a way out.  Surely this can't be all that exists in the afterlife? The randomly-generated monsters and dungeons become increasingly more difficult the farther you travel.

If this ever attracts any players, I plan to implement the following:
 - A new twist on "seasons."  Each season, your current power and progress will be stashed, and you essentially go back to a new character.  At the end of the season, everything you've accumulated is retained, and your stored resources are added back.
 - World bosses.
 - Random events orchestrated by yours truly.
 - An editor that allows you to build areas, items, and NPCs.  They can either be permanent, static objects or added to the pool of randomly-generated things.
 - A powerful C# scripting system.  You'll be able to add scripts to areas, items, and NPCs.  Mod approval will be required.  But re-usable functions, once approved, can be used again without requiring mod approval (so long as the script consists entirely of pre-approved functions).
 - Maybe WebRTC voice chat.  It'd be cool to add.
