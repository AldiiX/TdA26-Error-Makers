<script setup lang="ts">
    import type { Course } from "~/lib/types";
    import { computed } from "vue";
    import Button from "~/components/Button.vue";

    const props = defineProps<{ course: Course }>();

    
    const formattedCreatedAt = computed(() => {
        const raw = props.course?.createdAt;
        if (!raw) return "";
        const date = new Date(raw);
        return date.toLocaleDateString("cs-CZ", { year: "numeric", month: "short", day: "numeric" });
    });

    const formattedUpdatedAt = computed(() => {
        const raw = props.course?.updatedAt;
        if (!raw) return "";
        const date = new Date(raw);
        return date.toLocaleDateString("cs-CZ", { year: "numeric", month: "short", day: "numeric" });
    });
    
</script>

<template>
    <div :class="$style.container">
        <div :class="$style.content">
            <div :class="$style.imageContainer">
                <div :class="$style.image"></div>                
            </div>
            <div :class="$style.infoContainer">
                <h1 :class="[$style.nadpis, 'text-gradient']"> {{ course.name }}</h1>
                <p :class="$style.autor"> {{  }} Serhii Yavorskyi </p> <!-- autor -->
                <div :class="$style.date">
                    <p :class="$style.created">Vytvořeno: {{ formattedCreatedAt }}</p>
                    <p :class="$style.lastUpdate">Poslední úprava: {{ formattedUpdatedAt }}</p>
                </div>
            </div>
        </div>
        <div :class="$style.buttonsContainer">
            <div :class="$style.anotherInfo">
                <div :class="$style.views">
                    <div :class="$style.viewsIcon"></div>
                    <p>{{ }} 6</p>
                </div>
                <div :class="$style.rating">
                    <div :class="$style.ratingIcon"></div>
                    <p>{{ }} 5</p>
                </div>
            </div>
            
            <Button button-style="gradient" href="/courses/{uuid}" accent-color="primary">Začít</Button>
        </div>
    </div>
</template>

<style module lang="scss">
.container {
    display: flex;
    flex-direction: column;
    align-items: center;
    height: 100%;
    width: 100%;
    background-color: var(--background-color-secondary);
    border-radius: 16px;
    box-shadow: 12px 0 32px rgba(0, 0, 0, 0.1);
    
    .content{
        display: flex;
        flex-direction: column;
        height: 80%;
        width: 100%;
        
        
        .imageContainer{
            min-height: 200px;
            width: 100%;
            background-color: var(--accent-color-primary);
            overflow: hidden;
            border-radius: 16px;
            
            .image{
                
            }
        }
        
        .infoContainer{
            padding: 12px;
            
            h1{
                margin: 0 auto;
                font-size: 24px;
                
                
                // :TODO opravit bug z textem(gg overflowujou)
            }
            
            .autor{
                font-size: 16px;
                color: var(--text-color-secondary);
                margin: 4px 0;
            }
            
            .date{
                display: flex;
                gap: 16px;
                

                .created, .lastUpdate {
                    text-align: center;
                    font-size: 14px;
                    color: var(--text-color-secondary);
                }
            }
            
            
            
        }
    }
    
    .buttonsContainer{
        display: flex;
        justify-content: space-between;
        align-items: center;
        width: 100%;
        padding: 0 12px;
        margin: 0 auto;
        
        .anotherInfo{
            display: flex;
            gap: 8px;
            
            .views{
                display: flex;
                align-items: center;
                justify-content: space-between;
                gap: 4px;

                .viewsIcon{
                    mask-image: url("../../public/icons/views.svg");
                    mask-size: cover;
                    mask-position: center;
                    mask-repeat: no-repeat;

                    height: 24px;
                    width: 24px;
                    background-color: var(--text-color-secondary);
                }

                p{
                    font-size: 16px;
                    margin: 0 auto;
                }
            }

            .rating{
                display: flex;
                align-items: center;
                justify-content: space-between;
                gap: 4px;

                .ratingIcon{
                    mask-image: url("../../public/icons/star.svg");
                    mask-size: cover;
                    mask-position: center;
                    mask-repeat: no-repeat;

                    height: 24px;
                    width: 24px;
                    background-color: var(--text-color-secondary);

                }

                p{
                    font-size: 16px;
                    margin: 0 auto;
                }
            }
        }

        button{
            width: 50%;
        }
    }
}
</style>
