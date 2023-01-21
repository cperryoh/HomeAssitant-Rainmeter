# HomeAssitant-Rainmeter
## Set up
- Head over to your hassio server and create an api token by going to your use profile
- Put the .dll in your plugins folder
- Add this measure to your raimeter skin and fill in your values
  ```
  [hassio]
  Measure=Plugin
  Plugin=#SETTINGSPATH#\Plugins\HomeAssitantPlugin.dll
  auth=<api-key-from-hassio>
  server=<ip> #server ip
  ```
## Useage
### Configuration
|  Varible          | Description                                                          | Default |
|     :----:        |    :----:                                                            |  :----: |
| `auth`              | Auth token from home assistant                                     |`<empty>`|
| `server`            | Hassio server IP                                                   |`<empty>`|
| `path`(optional)    | Attribute path from entity                                         |`"state"`|
| `isInt`(optional)   | Set to `true` if the attribute or state you are pulling is a number|`false`  |
| `entityId`(optional)| Entity from home assistant to pull data from                       |`<empty>`|
### Showing data
To display data simply do this(if you are trying to use a meter that requires numerical data ensure isInt is true):
```
[hassio]
Measure=Plugin
Plugin=#SETTINGSPATH#\Plugins\HomeAssitantPlugin.dll
server=homeassistant.local
auth=<auth token>
entityId=switch.example
isInt=true
[example]
Meter=String
MeasureName=hassio
X=400
Y=70
FontSize=30
StringAlign=Center
Text=Entity: %1
```
### Path
To access attributes of an entity you have to specify the path within the json.
#### Example:
Say we have the following json of a weather object:
```
{
    "entity_id": "weather.home",
    "state": "sunny",
    "attributes": {
        "temperature": 75,
        "temperature_unit": "°F",
        "humidity": 65,
        "pressure": 30.03,
        "pressure_unit": "inHg",
        "wind_bearing": 202.7,
        "wind_speed": 3.6,
        "wind_speed_unit": "mph",
        "visibility_unit": "mi",
        "precipitation_unit": "in",
        "forecast": [
            {
                "condition": "sunny",
                "datetime": "2022-08-25T16:00:00+00:00",
                "wind_bearing": 219.9,
                "temperature": 87,
                "templow": 65,
                "wind_speed": 8.51,
                "precipitation": 0.0
            },
            {
                "condition": "cloudy",
                "datetime": "2022-08-26T16:00:00+00:00",
                "wind_bearing": 264.2,
                "temperature": 83,
                "templow": 69,
                "wind_speed": 6.03,
                "precipitation": 0.11
            },
            {
                "condition": "sunny",
                "datetime": "2022-08-27T16:00:00+00:00",
                "wind_bearing": 56.4,
                "temperature": 83,
                "templow": 65,
                "wind_speed": 7.83,
                "precipitation": 0.0
            },
            {
                "condition": "cloudy",
                "datetime": "2022-08-28T16:00:00+00:00",
                "wind_bearing": 177.2,
                "temperature": 88,
                "templow": 69,
                "wind_speed": 6.71,
                "precipitation": 0.0
            },
            {
                "condition": "rainy",
                "datetime": "2022-08-29T16:00:00+00:00",
                "wind_bearing": 239.6,
                "temperature": 92,
                "templow": 76,
                "wind_speed": 7.39,
                "precipitation": 0.76
            }
        ],
        "attribution": "Weather forecast from met.no, delivered by the Norwegian Meteorological Institute.",
        "friendly_name": "Forecast Home"
    },
    "last_changed": "2022-08-24T23:53:34.080002+00:00",
    "last_updated": "2022-08-25T01:55:34.989714+00:00",
    "context": {
        "id": "01GB9BCFED1JW1M8RCR5525Y2C",
        "parent_id": null,
        "user_id": null
    }
}
```
Note: to find the jsons of other entitys you may have,go to http://homeassistant:8123/developer-tools/state, any value you find in there will be prefaced by `attributes.` in the path. The plugin also outputs the json hassio gives to the rainmeter log file.
![image](https://user-images.githubusercontent.com/46071730/213832008-57dee21b-bd8c-4ed4-b40a-43ed545907f3.png)



If we wanted the forcast for 2022-08-26 the path would be the following, `attributes.forcast.1.condition`. If we wanted the next day, we would change the index from 1 to 2. By default path is set to state so it automatically returns the state of the entity
## Calling services
In order to call a service use bangs.

`[!CommandMeasure hassio "<domain>!<service>!<service data>"]`

For example, if we wanted to toggle a switch named fan we would use the following bang:

`[!CommandMeasure hassio "switch!toggle!{'entity_id': 'switch.fan'}"]`

Note: Single quotes **MUST** be used in the json for service data

If you are unsure about how to structure the service data json go to http://homeassistant:8123/developer-tools/service, build your service using the ui and then go to yaml mode and base the json off that. 

For example, take this service
```
service: light.turn_on
data:
  rgb_color:
    - 225
    - 0
    - 0
  transition: 24
target:
  entity_id: light.lamp
```

The json would be:
```
{
'rgb_color':[225,0,0],
'transition': 24,
'entity_id':'light.lamp'
}
```
And the bang would be:
```
LeftMouseUpAction=[!CommandMeasure hassio "light!turn_on!{'rgb_color':[255,0,0],'transition': 2,'entity_id':'light.lamp'}"]
```
