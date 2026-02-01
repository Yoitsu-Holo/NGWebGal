using NGWebGal.Driver.Data;
using NGWebGal.Services;

namespace NGWebGal.Driver.API;

/// <summary>
/// Controller API - Button, Slider
/// </summary>
public class ControllerAPI
{
    private readonly LayoutManager _layoutManager;

    public ControllerAPI(LayoutManager layoutManager)
    {
        _layoutManager = layoutManager;
    }

    #region Button

    public Response SetButton(ButtonBoxInfo info)
    {
        // TODO: Implement when layer creation API is ready
        return new Response { Type = ResponseType.Success };
    }

    public Response SetButtonImage(ButtonBoxImage image)
    {
        // TODO: Implement when layer access API is ready
        return new Response { Type = ResponseType.Success };
    }

    #endregion

    #region Slider

    public Response SetSlider(SliderBoxInfo info)
    {
        // TODO: Implement when layer creation API is ready
        return new Response { Type = ResponseType.Success };
    }

    public Response SetSliderImage(SliderBoxImage image)
    {
        // TODO: Implement when layer access API is ready
        return new Response { Type = ResponseType.Success };
    }

    public Response SetSliderTrackImage(SliderBoxTrackImage track)
    {
        // TODO: Implement when layer access API is ready
        return new Response { Type = ResponseType.Success };
    }

    #endregion
}
