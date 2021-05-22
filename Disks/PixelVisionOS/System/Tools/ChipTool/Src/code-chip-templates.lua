function ChipEditorTool:CreateChipTemplates()

  self.gpuChips = {
    {
      name = "PV8",
      spriteName = "chippv8gpu",
      type = "gpu",
      message = "You are about to apply the PV8 GPU chip settings. The new GPU will allow for a resolution of 256x240 after accounting for overscan, remove the sprite draw limit and replace the existing colors with a new 16 color set.",
      fields = {
        {
          name = "displayWidthInputData",
          value = 256
        },
        {
          name = "displayHeightInputData",
          value = 240
        },
        
        {
          name = "drawsInputData",
          value = 0
        },
        {
          name = "totalColorsInputData",
          value = 16
        },
        {
          name = "cpsInputData",
          value = 16
        },
      },
      colors = {
        "#2D1B2E",
        "#218A91",
        "#3CC2FA",
        "#9AF6FD",
        "#4A247C",
        "#574B67",
        "#937AC5",
        "#8AE25D",
        "#8E2B45",
        "#F04156",
        "#F272CE",
        "#D3C0A8",
        "#C5754A",
        "#F2A759",
        "#F7DB53",
        "#F9F4EA"
      },
      paletteMode = false
    },
    {
      name = "Fami",
      spriteName = "chipfamigpu",
      type = "gpu",
      message = "You are about to apply the Fami GPU chip settings. The new GPU will allow for a resolution of 240x232 after accounting for overscan, limit the number of sprites on the screen to 64, set the colors per sprite to 4 and replace the existing colors with a new 55 color set.",
      fields = {
        {
          name = "displayWidthInputData",
          value = 256
        },
        {
          name = "displayHeightInputData",
          value = 240
        },
        {
          name = "drawsInputData",
          value = 64
        },
        {
          name = "totalColorsInputData",
          value = 55
        },
        {
          name = "cpsInputData",
          value = 3
        }
      },
      colors = {
        "#7C7C7C",
        "#0000FC",
        "#0000BC",
        "#4428BC",
        "#940084",
        "#A80020",
        "#A81000",
        "#881400",
        "#503000",
        "#007800",
        "#006800",
        "#005800",
        "#004058",
        "#000000",
        "#BCBCBC",
        "#0078F8",
        "#0058F8",
        "#6844FC",
        "#D800CC",
        "#E40058",
        "#F83800",
        "#E45C10",
        "#AC7C00",
        "#00B800",
        "#00A800",
        "#00A844",
        "#008888",
        "#F8F8F8",
        "#3CBCFC",
        "#6888FC",
        "#9878F8",
        "#F878F8",
        "#F85898",
        "#F87858",
        "#FCA044",
        "#F8B800",
        "#B8F818",
        "#58D854",
        "#58F898",
        "#00E8D8",
        "#787878",
        "#FCFCFC",
        "#A4E4FC",
        "#B8B8F8",
        "#D8B8F8",
        "#F8B8F8",
        "#F8A4C0",
        "#F0D0B0",
        "#FCE0A8",
        "#F8D878",
        "#D8F878",
        "#B8F8B8",
        "#B8F8D8",
        "#00FCFC",
        "#F8D8F8"
      },
      paletteMode = true,
      palette = {
        {
          "#000000",
          "#7C7C7C",
          "#F8F8F8"
        }
      }
    },
    {
      name = "Mastr",
      spriteName = "chipmastrgpu",
      type = "gpu",
      message = "You are about to apply the Mastr GPU chip settings. The new GPU will allow for a resolution of 240x184 after accounting for overscan, limit the number of sprites on the screen to 64, set the colors per sprite to 6 and replace the existing colors with a new 64 color set.",
      fields = {
        {
          name = "displayWidthInputData",
          value = 256
        },
        {
          name = "displayHeightInputData",
          value = 192
        },
        
        {
          name = "drawsInputData",
          value = 64
        },
        {
          name = "totalColorsInputData",
          value = 64
        },
        {
          name = "cpsInputData",
          value = 6
        },
      },
      colors = {
        "#000000",
        "#560000",
        "#AD0000",
        "#FF0000",
        "#000258",
        "#560057",
        "#AE0056",
        "#FF0053",
        "#005500",
        "#565500",
        "#AD5400",
        "#FF5400",
        "#005555",
        "#555555",
        "#AD5554",
        "#FF5450",
        "#00AA00",
        "#51AA00",
        "#ABAA00",
        "#FFA900",
        "#00AA4E",
        "#50AA4D",
        "#ABAA4B",
        "#FFA948",
        "#00FF00",
        "#47FF00",
        "#A7FF00",
        "#FFFF00",
        "#00FF3A",
        "#46FF3A",
        "#A7FF37",
        "#FFFF32",
        "#0007AF",
        "#5506AF",
        "#AD04AE",
        "#FF00AD",
        "#000FFF",
        "#520FFF",
        "#AC0DFF",
        "#FF0AFF",
        "#0056AE",
        "#5456AE",
        "#AC55AD",
        "#FF55AC",
        "#0057FF",
        "#5057FF",
        "#AA56FF",
        "#FF56FF",
        "#00ABAB",
        "#4FABAB",
        "#AAAAAA",
        "#FFAAA9",
        "#00ABFF",
        "#4BABFF",
        "#A9ABFF",
        "#FFABFF",
        "#00FFA4",
        "#44FFA4",
        "#A6FFA3",
        "#FFFFA3",
        "#00FFFF",
        "#40FFFF",
        "#A5FFFF",
        "#FFFFFF",
      },
      paletteMode = true,
      palette = {
        {
          "#000000",
          "#000258",
          "#0007AF",
          "#0057FF",
          "#A5FFFF",
          "#FFFFFF"
        }
      }
    },
    {
      name = "GBoy",
      spriteName = "chipgboygpu",
      type = "gpu",
      message = "You are about to apply the GBoy GPU chip settings. The new GPU will allow for a resolution of 152x136 after accounting for overscan, limit the number of sprites on the screen to 40, set the colors per sprite to 4 and replace the existing colors with a new 4 color set.",
      fields = {
        {
          name = "displayWidthInputData",
          value = 160
        },
        {
          name = "displayHeightInputData",
          value = 144
        },
        
        {
          name = "drawsInputData",
          value = 40
        },
        {
          name = "totalColorsInputData",
          value = 4
        },
        {
          name = "cpsInputData",
          value = 3
        }
      },
      colors = {
        "#162b14",
        "#376142",
        "#61976f",
        "#c2e199"
      },
      paletteMode = true,
      palette = {
        {
          "#162b14",
          "#376142",
          "#c2e199"
        }
      }
    },
    -- {
    --   -- TODO This chip needs to be configured
    --   name = "Gear",
    --   spriteName = "chipgeargpu",
    --   message = "You are about to apply the Gear GPU chip settings.",
    --   fields = {
    --     {
    --       name = "displayWidthInputData",
    --       value = 160
    --     },
    --     {
    --       name = "displayHeightInputData",
    --       value = 144
    --     },
    --     {
    --       name = "overscanRightInputData",
    --       value = 1
    --     },
    --     {
    --       name = "overscanBottomInputData",
    --       value = 1
    --     },
    --     {
    --       name = "drawsInputData",
    --       value = 40
    --     },
    --     {
    --       name = "totalColorsInputData",
    --       value = 4
    --     },
    --     {
    --       name = "cpsInputData",
    --       value = 3
    --     }
    --   },
    --   colors = {}
    -- },
    {
      -- TODO This chip needs to be configured
      name = "Pico",
      spriteName = "chippicogpu",
      type = "gpu",
      message = "You are about to apply the Pico GPU chip settings. The new GPU will allow for a resolution of 128x128 after accounting for overscan, remove the sprite limit, set the colors per sprite to 16 and replace the existing colors with a new 16 color set.",
      fields = {
        {
          name = "displayWidthInputData",
          value = 128
        },
        {
          name = "displayHeightInputData",
          value = 128
        },
        
        {
          name = "drawsInputData",
          value = 0
        },
        {
          name = "totalColorsInputData",
          value = 16
        },
        {
          name = "cpsInputData",
          value = 16
        }
      },
      colors = {
        "#000000",
        "#1D2B53",
        "#7E2553",
        "#008751",
        "#AB5236",
        "#5F574F",
        "#C2C3C7",
        "#FFF1E8",
        "#FF004D",
        "#FFA300",
        "#FFEC27",
        "#00E436",
        "#29ADFF",
        "#83769C",
        "#FF77A8",
        "#FFCCAA"
      },
      paletteMode = false
    },
    {
      -- TODO This chip needs to be configured
      name = "ArduKid",
      spriteName = "chipardukidgpu",
      type = "gpu",
      message = "You are about to apply the ArduKid GPU chip settings. The new GPU will allow for a resolution of 128x64 after accounting for overscan, remove the sprite limit, set the colors per sprite to 3 and replace the existing colors with a new 2 color set.",
      fields = {
        {
          name = "displayWidthInputData",
          value = 128
        },
        {
          name = "displayHeightInputData",
          value = 64
        },
        
        {
          name = "drawsInputData",
          value = 0
        },
        {
          name = "totalColorsInputData",
          value = 4
        },
        {
          name = "cpsInputData",
          value = 2
        }
      },
      colors = {
        "#000000",
        "#FFFFFF"
      },
      paletteMode = false
    },
    {
      name = "CGA",
      spriteName = "chipcgagpu",
      type = "gpu",
      message = "You are about to apply the Pico GPU chip settings. The new GPU will allow for a resolution of 320x200 after accounting for overscan, remove the sprite limit, set the colors per sprite to 4 and replace the existing colors with a new 16 color set.",
      fields = {
        {
          name = "displayWidthInputData",
          value = 320
        },
        {
          name = "displayHeightInputData",
          value = 200
        },
        
        {
          name = "drawsInputData",
          value = 0
        },
        {
          name = "totalColorsInputData",
          value = 16
        },
        {
          name = "cpsInputData",
          value = 8
        }
      },
      colors = {
        "#000000",
        "#0000AA",
        "#00AA00",
        "#00AAAA",
        "#AA0000",
        "#AA00AA",
        "#AA5500",
        "#AAAAAA",
        "#555555",
        "#5555FF",
        "#55FF55",
        "#55FFFF",
        "#FF5555",
        "#FF55FF",
        "#FFFF55",
        "#FFFFFF"
      },
      paletteMode = true,
      palette = {
        {
          "#000000",
          "#0000AA",
          "#00AA00",
          "#00AAAA",
          "#AA0000",
          "#AA00AA",
          "#AA5500",
          "#AAAAAA",
        },
        {
          "#555555",
          "#5555FF",
          "#55FF55",
          "#55FFFF",
          "#FF5555",
          "#FF55FF",
          "#FFFF55",
          "#FFFFFF"
        }
      }
    }

  }

  self.cartChips = {
    {
      name = "PV8",
      spriteName = "chippv8cart",
      type = "cart",
      message = "You are about to apply the PV8 Cart settings.",
      fields = {
        {
          name = "sizeInputData",
          value = 512
        },
        {
          name = "spritePagesInputData",
          value = 8
        },
        {
          name = "mapWidthInputData",
          value = 256
        },
        {
          name = "mapHeightInputData",
          value = 256
        }
      }
    },
    {
      name = "Fami",
      spriteName = "chipfamicart",
      type = "cart",
      message = "You are about to apply the Fami Cart settings.",
      fields = {
        {
          name = "sizeInputData",
          value = 384
        },
        {
          name = "spritePagesInputData",
          value = 4
        },
        {
          name = "mapWidthInputData",
          value = 256
        },
        {
          name = "mapHeightInputData",
          value = 256
        }
      }
    },
    {
      name = "Mastr",
      spriteName = "chipmastrcart",
      type = "cart",
      message = "You are about to apply the Mastr Cart settings.",
      fields = {
        {
          name = "sizeInputData",
          value = 256
        },
        {
          name = "spritePagesInputData",
          value = 2
        },
        {
          name = "mapWidthInputData",
          value = 256
        },
        {
          name = "mapHeightInputData",
          value = 256
        }
      }
    },
    {
      name = "GBoy",
      spriteName = "chipgboycart",
      type = "cart",
      message = "You are about to apply the GBoy Cart settings.",
      fields = {
        {
          name = "sizeInputData",
          value = 128
        },
        {
          name = "spritePagesInputData",
          value = 2
        },
        {
          name = "mapWidthInputData",
          value = 128
        },
        {
          name = "mapHeightInputData",
          value = 128
        }
      }
    },
    -- {
    --   -- TODO Need to configure these values
    --   name = "Gear",
    --   spriteName = "chipgearcart",
    --   message = "You are about to apply the Gear Cart settings.",
    --   fields = {
    --     {
    --       name = "sizeInputData",
    --       value = 128
    --     },
    --     {
    --       name = "spritePagesInputData",
    --       value = 2
    --     },
    --     {
    --       name = "mapWidthInputData",
    --       value = 128
    --     },
    --     {
    --       name = "mapHeightInputData",
    --       value = 128
    --     }
    --   }
    -- },
    {
      -- TODO Need to configure these values
      name = "Pico",
      spriteName = "chippicocart",
      type = "cart",
      message = "You are about to apply the Pico Cart settings.",
      fields = {
        {
          name = "sizeInputData",
          value = 20
        },
        {
          name = "spritePagesInputData",
          value = 1
        },
        {
          name = "mapWidthInputData",
          value = 128
        },
        {
          name = "mapHeightInputData",
          value = 32
        }
      }
    },
    {
      -- TODO Need to configure these values
      name = "ArduKid",
      type = "cart",
      spriteName = "chipardukidcart",
      message = "You are about to apply the ArduKid Cart settings.",
      fields = {
        {
          name = "sizeInputData",
          value = 30
        },
        {
          name = "spritePagesInputData",
          value = 2
        },
        {
          name = "mapWidthInputData",
          value = 64
        },
        {
          name = "mapHeightInputData",
          value = 64
        }
      }
    }
  }

  self.soundChips = {
    {
      name = "PV8",
      spriteName = "chippv8sound",
      type = "sound",
      message = "You are about to apply the PV8 sound chip settings. This chip uses 5 channels capable of square, saw, noise, triangle and wav sample on any channel.",
      channels = { - 1, - 1, - 1, - 1, - 1},
      fields = {
        {
          name = "songTotalInputData",
          value = 32
        },
        {
          name = "soundTotalInputData",
          value = 32
        },
        {
          name = "channelTotalInputData",
          value = 5
        },
        {
          name = "loopTotalInputData",
          value = 24
        },
        {
          name = "tracksTotalInputData",
          value = 4
        }
      }
    },
    {
      name = "Fami",
      spriteName = "chipfamisound",
      type = "sound",
      message = "You are about to apply the Fami sound chip settings. This chip uses 5 channels: two square, one triangle, one noise, and one wav sample.",
      channels = { 0, 0, 4, 3, 5},
      fields = {
        {
          name = "songTotalInputData",
          value = 16
        },
        {
          name = "soundTotalInputData",
          value = 32
        },
        {
          name = "channelTotalInputData",
          value = 5
        },
        {
          name = "loopTotalInputData",
          value = 16
        },
        {
          name = "tracksTotalInputData",
          value = 4
        }
      }
    },
    {
      name = "Mastr",
      spriteName = "chipmastrsound",
      type = "sound",
      message = "You are about to apply the Mastr sound chip settings. This chip uses 4 channels: three square, and one noise.",
      channels = { 0, 0, 0, 3},
      fields = {
        {
          name = "songTotalInputData",
          value = 24
        },
        {
          name = "soundTotalInputData",
          value = 28
        },
        {
          name = "channelTotalInputData",
          value = 4
        },
        {
          name = "loopTotalInputData",
          value = 16
        },
        {
          name = "tracksTotalInputData",
          value = 3
        }
      }
    },
    {
      name = "GBoy",
      spriteName = "chipgboysound",
      type = "sound",
      channels = { 0, 0, 4, 3},
      message = "You are about to apply the GBoy sound chip settings. This chip uses 4 channels: two square, one wav sample, and one noise.",
      fields = {
        {
          name = "songTotalInputData",
          value = 8
        },
        {
          name = "soundTotalInputData",
          value = 16
        },
        {
          name = "channelTotalInputData",
          value = 4
        },
        {
          name = "loopTotalInputData",
          value = 8
        },
        {
          name = "tracksTotalInputData",
          value = 2
        }
      }
    },
    -- {
    --   -- TODO need to configure these values
    --   name = "Gear",
    --   spriteName = "chipgearsound",
    --   message = "You are about to apply the Gear sound chip settings.",
    --   fields = {
    --     {
    --       name = "songTotalInputData",
    --       value = 8
    --     },
    --     {
    --       name = "soundTotalInputData",
    --       value = 16
    --     },
    --     {
    --       name = "channelTotalInputData",
    --       value = 3
    --     },
    --     {
    --       name = "loopTotalInputData",
    --       value = 8
    --     },
    --     {
    --       name = "tracksTotalInputData",
    --       value = 2
    --     }
    --   },
    -- },
    {
      -- TODO need to configure these values
      name = "Pico",
      spriteName = "chippicosound",
      type = "sound",
      message = "You are about to apply the Pico sound chip settings. his chip uses 4 channels capable of square, saw, noise, triangle and wav sample on any channel.",
      fields = {
        {
          name = "songTotalInputData",
          value = 4
        },
        {
          name = "soundTotalInputData",
          value = 16
        },
        {
          name = "channelTotalInputData",
          value = 4
        },
        {
          name = "loopTotalInputData",
          value = 8
        },
        {
          name = "tracksTotalInputData",
          value = 4
        }
      }
    },
    {
      -- TODO need to configure these values
      name = "ArduKid",
      spriteName = "chipardukidsound",
      type = "sound",
      message = "You are about to apply the ArduKid sound chip settings. his chip uses 2 channels capable of square, saw, noise, triangle and wav sample on any channel.",
      fields = {
        {
          name = "songTotalInputData",
          value = 4
        },
        {
          name = "soundTotalInputData",
          value = 16
        },
        {
          name = "channelTotalInputData",
          value = 2
        },
        {
          name = "loopTotalInputData",
          value = 8
        },
        {
          name = "tracksTotalInputData",
          value = 1
        }
      }
    }

  }

end