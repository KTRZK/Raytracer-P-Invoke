#include "rtweekend.h"
#include "hittable_list.h"
#include "material.h"
#include "sphere.h"
#include "color.h"
#include "camera.h"
#include <vector>
#include <cstring>
#include <memory>
#include <cstdint>

#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "external/stb_image_write.h"

#ifdef _WIN32
    #define RT_EXPORT __declspec(dllexport)
#else
    #define RT_EXPORT __attribute__((visibility("default")))
#endif

extern "C" {

struct CameraConfig {
    double aspect_ratio;
    int image_width;
    int samples_per_pixel;
    int max_depth;
    double vfov;
    double lookfrom_x, lookfrom_y, lookfrom_z;
    double lookat_x, lookat_y, lookat_z;
    double vup_x, vup_y, vup_z;
    double defocus_angle;
    double focus_dist;
};

typedef void (*RenderCallback)(int samples, uint8_t* buffer);

RT_EXPORT void* CreateScene() {
    return new hittable_list();
}

RT_EXPORT void DestroyScene(void* scene) {
    delete static_cast<hittable_list*>(scene);
}

RT_EXPORT void SceneClear(void* scene) {
    static_cast<hittable_list*>(scene)->clear();
}

RT_EXPORT void SceneAddSphere(void* scene, double cx, double cy, double cz, double radius, void* mat) {
    auto* list = static_cast<hittable_list*>(scene);
    auto* material_ptr = static_cast<shared_ptr<material>*>(mat);
    list->add(make_shared<sphere>(point3(cx, cy, cz), radius, *material_ptr));
}

RT_EXPORT void* CreateLambertian(double r, double g, double b) {
    return new shared_ptr<material>(make_shared<lambertian>(color(r, g, b)));
}

RT_EXPORT void* CreateMetal(double r, double g, double b, double fuzz) {
    return new shared_ptr<material>(make_shared<metal>(color(r, g, b), fuzz));
}

RT_EXPORT void* CreateDielectric(double refraction_index) {
    return new shared_ptr<material>(make_shared<dielectric>(refraction_index));
}

RT_EXPORT void DestroyMaterial(void* mat) {
    delete static_cast<shared_ptr<material>*>(mat);
}

RT_EXPORT void RenderScene(void* scene, CameraConfig config, uint8_t* buffer, RenderCallback callback) {
    auto* world = static_cast<hittable_list*>(scene);
    
    camera cam;
    cam.aspect_ratio = config.aspect_ratio;
    cam.image_width = config.image_width;
    cam.samples_per_pixel = config.samples_per_pixel;
    cam.max_depth = config.max_depth;
    cam.vfov = config.vfov;
    cam.lookfrom = point3(config.lookfrom_x, config.lookfrom_y, config.lookfrom_z);
    cam.lookat = point3(config.lookat_x, config.lookat_y, config.lookat_z);
    cam.vup = vec3(config.vup_x, config.vup_y, config.vup_z);
    cam.defocus_angle = config.defocus_angle;
    cam.focus_dist = config.focus_dist;
    
    cam.render(*world, buffer, callback);
}

RT_EXPORT int GetImageHeight(int width, double aspect_ratio) {
    int height = static_cast<int>(width / aspect_ratio);
    return (height < 1) ? 1 : height;
}

RT_EXPORT int SavePng(const char* filename, int width, int height, uint8_t* data) {
    return stbi_write_png(filename, width, height, 4, data, width * 4);
}

} // extern "C"
