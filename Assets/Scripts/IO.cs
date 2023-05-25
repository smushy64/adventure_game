using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IO {

    const string SETTINGS_PATH = "./settings.ini";

    public static void debug_write_file() {
        using( StreamWriter writer = new StreamWriter("./settings.txt") ) {
            writer.WriteLine("Hello World");
        }
    }

    public static void serialize_game_options( GameOptions options ) {
        using( StreamWriter writer = new StreamWriter( SETTINGS_PATH ) ) {
            writer.WriteLine( "[graphics]" );
            writer.WriteLine( "width  = " + options.window_width );
            writer.WriteLine( "height = " + options.window_height );
            writer.WriteLine( "resolution_scale = " + options.resolution_scale );

            writer.WriteLine("; Uses Unity's enum values");
            writer.WriteLine("; ExclusiveFullScreen = 0");
            writer.WriteLine("; FullScreenWindow    = 1");
            writer.WriteLine("; MaximizedWindow     = 2");
            writer.WriteLine("; Windowed            = 3");
            writer.WriteLine( "fullscreen_mode  = " + (int)options.fullscreen_mode );

            writer.WriteLine("");

            writer.WriteLine( "[controls]" );
            writer.WriteLine( "invert_look_x = " + options.invert_look_x.ToString().ToLower() );
            writer.WriteLine( "invert_look_y = " + options.invert_look_y.ToString().ToLower() );
            writer.WriteLine( "look_sensitivity_x = " + options.look_sensitivity_x );
            writer.WriteLine( "look_sensitivity_y = " + options.look_sensitivity_y );
        }
    }

    static GameOptions parse_game_options() {
        GameOptions result = new GameOptions();

        using( StreamReader reader = new StreamReader( SETTINGS_PATH ) ) {
            string line;
            while( (line = reader.ReadLine()) != null ) {
                string[] words = line.Split( ' ' );
                const int LINE_WORD_COUNT = 3;
                if(
                    words.Length < LINE_WORD_COUNT ||
                    words.Length > LINE_WORD_COUNT
                ) {
                    continue;
                }
                switch( words[0] ) {
                    case "width": {
                        if( Int32.TryParse( words[2], out int width ) ) {
                            result.window_width = width;
                        }
                    } break;
                    case "height": {
                        if( Int32.TryParse( words[2], out int height ) ) {
                            result.window_height = height;
                        }
                    } break;
                    case "resolution_scale": {
                        if( Single.TryParse( words[2], out float resolution_scale ) ) {
                            result.resolution_scale = resolution_scale;
                        }
                    } break;
                    case "fullscreen_mode": {
                        if( Int32.TryParse( words[2], out int fullscreen_mode ) ) {
                            if( fullscreen_mode >= 0 && fullscreen_mode <= (int)FullScreenMode.Windowed ) {
                                result.fullscreen_mode = (FullScreenMode)fullscreen_mode;
                            }
                        }
                    } break;
                    case "invert_look_x": {
                        if( Boolean.TryParse( words[2], out bool invert_look_x_bool ) ) {
                            result.invert_look_x = invert_look_x_bool;
                        } else if( Int32.TryParse( words[2], out int invert_look_x_int ) ) {
                            result.invert_look_x = invert_look_x_int != 0;
                        }
                    } break;
                    case "invert_look_y": {
                        if( Boolean.TryParse( words[2], out bool invert_look_y_bool ) ) {
                            result.invert_look_y = invert_look_y_bool;
                        } else if( Int32.TryParse( words[2], out int invert_look_y_int ) ) {
                            result.invert_look_y = invert_look_y_int != 0;
                        }
                    } break;
                    case "look_sensitivity_x": {
                        if( Single.TryParse( words[2], out float look_sensitivity_x ) ) {
                            result.look_sensitivity_x = look_sensitivity_x;
                        }
                    } break;
                    case "look_sensitivity_y": {
                        if( Single.TryParse( words[2], out float look_sensitivity_y ) ) {
                            result.look_sensitivity_y = look_sensitivity_y;
                        }
                    } break;
                    default: continue;
                }
            }
        }

        return result;
    }

    public static GameOptions deserialize_game_options() {
        
        if( File.Exists( SETTINGS_PATH ) ) {
            return parse_game_options();
        } else {
            GameOptions result = new GameOptions();
            serialize_game_options( result );
            return result;
        }

    }


}
