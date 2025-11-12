<script setup lang="ts">
    import type { Course } from "#shared/types";
    import Button from "~/components/Button.vue";
    import timeAgoString from "#shared/utils/timeAgoString";

    const props = defineProps<{ course: Course }>();
</script>

<template>
    <div :class="$style.container">
        <div :class="$style.top">
            <div :class="$style.imageContainer">
                <div :class="$style.image"></div>                
            </div>
        </div>
        <div :class="$style.bottom">
            <div :class="$style.infoContainer">
                <h1 :class="[$style.nadpis, 'text-gradient']"> {{ course.name }}</h1>
                <p :class="$style.autor"> {{  }} Serhii Yavorskyi </p> <!-- autor -->
                <div :class="$style.date">
                    <p :class="$style.created">Vytvořeno: {{ timeAgoString(course.createdAt) }}</p>
                    <p :class="$style.lastUpdate">Poslední úprava: {{ timeAgoString(course.updatedAt) }}</p>
                </div>
            </div>
            <div :class="$style.buttonsContainer">
                <div :class="$style.anotherInfo">
                    <div :class="$style.info">
                        <div style="mask-image: url(/icons/star.svg)"></div>
                        <p>{{ }} 6</p>
                    </div>
                    <div :class="$style.info">
                        <div style="mask-image: url(/icons/views.svg)"></div>
                        <p>{{ }} 5</p>
                    </div>
                </div>

                <Button button-style="primary" href="/courses/{uuid}" accent-color="secondary">Začít</Button>
            </div>
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
    
    .top{
        
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
    }

    .bottom{
        display: flex;
        flex-direction: column;
        width: 100%;
        flex-grow: 1; 
        
        .infoContainer{
            display: flex;
            flex-direction: column;
            padding: 16px;
            flex-grow: 1;

            h1{
                margin: 0;
                font-size: 24px;
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
        
        .buttonsContainer{
            display: flex;
            justify-content: space-between;
            align-items: end;
            width: 100%;
            padding: 12px;
            margin-top: auto;

            .anotherInfo{
                display: flex;
                gap: 16px;

                .info {
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    gap: 4px;

                    >div {
                        mask-image: url("../../../public/icons/views.svg");
                        mask-size: cover;
                        mask-position: center;
                        mask-repeat: no-repeat;
                        height: 16px;
                        width: 16px;
                        background-color: var(--text-color-secondary);
                    }

                    >p {
                        font-size: 16px;
                        margin: 0;
                    }
                }
            }

            button{
                width: 50%;
            }
        }
    }
    
}
</style>
